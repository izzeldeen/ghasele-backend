using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Ghasele.Application.DTOs;
using Ghasele.Application.Exceptions;
using Ghasele.Application.Interfaces;
using Ghasele.Application.Localization;
using Ghasele.Domain.Entities;
using Ghasele.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;

namespace Ghasele.Application.Services
{
    public class AuthService : IAuthService
    {
        private const string AppleIssuer = "https://appleid.apple.com";
        private const string AppleKeysUrl = "https://appleid.apple.com/auth/keys";

        // Reused across requests; Apple's key set is fetched on demand.
        private static readonly HttpClient _httpClient = new HttpClient();

        private readonly IUserRepository _userRepository;
        private readonly IPendingRegistrationRepository _pendingRegistrationRepository;
        private readonly IConfiguration _configuration;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IFirebaseAuthService _firebaseAuthService;
        private readonly IErrorLocalizer _errorLocalizer;
        private readonly ICurrentLanguageProvider _language;

        public AuthService(
            IUserRepository userRepository,
            IPendingRegistrationRepository pendingRegistrationRepository,
            IConfiguration configuration,
            IWhatsAppService whatsAppService,
            IFirebaseAuthService firebaseAuthService,
            IErrorLocalizer errorLocalizer,
            ICurrentLanguageProvider language)
        {
            _userRepository = userRepository;
            _pendingRegistrationRepository = pendingRegistrationRepository;
            _configuration = configuration;
            _whatsAppService = whatsAppService;
            _firebaseAuthService = firebaseAuthService;
            _errorLocalizer = errorLocalizer;
            _language = language;
        }

        // How long the confirmed OTP stays good for the name/password step before the user has to
        // request a fresh code.
        private static readonly TimeSpan CompleteRegistrationWindow = TimeSpan.FromMinutes(30);

        // Step 1: the user gives only a phone number. We stash it in PendingRegistrations with a
        // fresh OTP - no User, no password, no name yet. An unverified phone number should never
        // occupy a real account.
        public async Task<RegisterResponse> StartRegistrationAsync(string phoneNumber)
        {
            var existingUser = await _userRepository.GetByPhoneNumberAsync(phoneNumber);
            if (existingUser != null)
            {
                throw new AppException(ErrorCodes.PhoneAlreadyExists, 409);
            }

            var otp = GenerateOtp();
            var otpExpiry = DateTime.UtcNow.AddMinutes(10);

            var pending = await _pendingRegistrationRepository.GetByPhoneNumberAsync(phoneNumber);
            if (pending == null)
            {
                pending = new PendingRegistration
                {
                    PhoneNumber = phoneNumber,
                    Otp = otp,
                    OtpExpiry = otpExpiry
                };
                await _pendingRegistrationRepository.AddAsync(pending);
            }
            else
            {
                // Restarting before finishing a previous attempt: issue a new code and drop any
                // earlier verified state so the flow starts clean.
                pending.Otp = otp;
                pending.OtpExpiry = otpExpiry;
                pending.IsOtpVerified = false;
                pending.PasswordHash = null;
                pending.FullName = null;
                await _pendingRegistrationRepository.UpdateAsync(pending);
            }

            await DispatchOtpAsync(phoneNumber, otp);

            return new RegisterResponse(phoneNumber, _errorLocalizer.Localize(ErrorCodes.OtpSent, _language.Language));
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByPhoneNumberAsync(request.PhoneNumber);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                // Deliberately 400, not 401: the admin client treats every 401 as an expired
                // session and force-logs-out, which would be wrong for a failed login attempt.
                throw new AppException(ErrorCodes.InvalidCredentials);
            }

            var token = GenerateJwtToken(user);

            return new AuthResponse(token, user.Id, user.Username, user.Email, user.FullName, user.PhoneNumber, user.IsPhoneVerified, user.Role.ToString());
        }

        public async Task<AuthResponse> AppleSignInAsync(AppleSignInRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.IdentityToken))
            {
                throw new AppException(ErrorCodes.AppleTokenMissing);
            }

            var (appleUserId, tokenEmail) = await ValidateAppleIdentityTokenAsync(request.IdentityToken);
            var email = !string.IsNullOrWhiteSpace(tokenEmail) ? tokenEmail : request.Email;

            // 1) Existing Apple user, 2) existing account with the same email (link it),
            // 3) brand new user.
            var user = await _userRepository.GetByAppleUserIdAsync(appleUserId);

            if (user == null && !string.IsNullOrWhiteSpace(email))
            {
                user = await _userRepository.GetByEmailAsync(email);
            }

            if (user == null)
            {
                var shortId = appleUserId.Length > 8 ? appleUserId.Substring(0, 8) : appleUserId;
                user = new User
                {
                    Id = Guid.NewGuid(),
                    AppleUserId = appleUserId,
                    Email = email,
                    Username = !string.IsNullOrWhiteSpace(email) ? email! : $"apple_{shortId}",
                    FullName = !string.IsNullOrWhiteSpace(request.FullName)
                        ? request.FullName!
                        : (email ?? "Apple User"),
                    // Apple accounts have no phone/password; keep NOT NULL columns satisfied.
                    PhoneNumber = string.Empty,
                    PasswordHash = string.Empty,
                    IsPhoneVerified = false
                };
                await _userRepository.AddAsync(user);
            }
            else if (string.IsNullOrEmpty(user.AppleUserId))
            {
                // Link Apple to a pre-existing (e.g. phone-registered) account.
                user.AppleUserId = appleUserId;
                if (string.IsNullOrEmpty(user.Email))
                {
                    user.Email = email;
                }
                await _userRepository.UpdateAsync(user);
            }

            var token = GenerateJwtToken(user);

            return new AuthResponse(token, user.Id, user.Username, user.Email, user.FullName, user.PhoneNumber, user.IsPhoneVerified, user.Role.ToString());
        }

        /// <summary>
        /// Signs a user in from a client-side Firebase Phone Authentication result.
        /// </summary>
        /// <remarks>
        /// Google has already sent the SMS and checked the code before this is called, so a token
        /// that verifies is sufficient proof of phone ownership and there is no password step.
        /// This runs alongside - not instead of - the WhatsApp OTP registration flow: a number that
        /// already has an account is matched, never duplicated.
        /// </remarks>
        public async Task<AuthResponse> FirebaseLoginAsync(FirebaseLoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.IdToken))
            {
                throw new AppException(ErrorCodes.FirebaseTokenMissing);
            }

            var phoneNumber = await _firebaseAuthService.VerifyIdTokenAndGetPhoneNumberAsync(request.IdToken);
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                // 401, not 400: the request was well formed, the credential was simply not accepted.
                throw new AppException(ErrorCodes.FirebaseTokenInvalid, 401);
            }

            // Firebase returns E.164 (+962...), which is the same form signup_screen.dart sends and
            // the same form already stored on User.PhoneNumber - so an existing WhatsApp-registered
            // user is found here rather than duplicated.
            var user = await _userRepository.GetByPhoneNumberAsync(phoneNumber);

            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    PhoneNumber = phoneNumber,
                    Username = phoneNumber,
                    FullName = phoneNumber,
                    // This flow never sets a password. The account is reachable only through
                    // Firebase sign-in until the user chooses one.
                    PasswordHash = string.Empty,
                    // Firebase verified the number before issuing the token, so unlike the WhatsApp
                    // flow there is no separate confirmation step left to perform.
                    IsPhoneVerified = true,
                    Role = UserRole.Client
                };

                await _userRepository.AddAsync(user);
            }
            else if (!user.IsPhoneVerified)
            {
                // Pre-existing account whose number Firebase has now proven.
                user.IsPhoneVerified = true;
                await _userRepository.UpdateAsync(user);
            }

            var token = GenerateJwtToken(user);

            return new AuthResponse(token, user.Id, user.Username, user.Email, user.FullName, user.PhoneNumber, user.IsPhoneVerified, user.Role.ToString());
        }

        // Verifies the identity token is a genuine, unexpired Apple token issued for
        // one of our configured client ids, then returns its subject and email.
        private async Task<(string Sub, string? Email)> ValidateAppleIdentityTokenAsync(string identityToken)
        {
            var allowedAudiences = _configuration.GetSection("AppleAuth:ClientIds")
                .GetChildren()
                .Select(c => c.Value)
                .Where(v => !string.IsNullOrEmpty(v))
                .ToArray();

            if (allowedAudiences.Length == 0)
            {
                throw new Exception("AppleAuth:ClientIds is not configured on the server.");
            }

            string keysJson;
            try
            {
                keysJson = await _httpClient.GetStringAsync(AppleKeysUrl);
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not fetch Apple public keys: {ex.Message}");
            }

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = AppleIssuer,
                ValidateAudience = true,
                ValidAudiences = allowedAudiences,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = new JsonWebKeySet(keysJson).GetSigningKeys()
            };

            var handler = new JwtSecurityTokenHandler();

            // By default this handler rewrites inbound JWT claim names to the legacy
            // WS-* URIs, so Apple's "sub" arrives as ".../identity/claims/nameidentifier"
            // and "email" as ".../claims/emailaddress". Looking them up by their real
            // names then returns nothing, which reads as a malformed token. Clearing the
            // map keeps Apple's claim names exactly as they are on the wire.
            handler.InboundClaimTypeMap.Clear();

            ClaimsPrincipal principal;
            try
            {
                principal = handler.ValidateToken(identityToken, validationParameters, out _);
            }
            catch (Exception ex)
            {
                throw new AppException(ErrorCodes.AppleTokenInvalid, 401, ex);
            }

            var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(sub))
            {
                throw new AppException(ErrorCodes.AppleTokenNoSubject, 401);
            }

            var email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            return (sub, email);
        }

        // Step 2: confirm the OTP. This no longer creates the account - it just marks the pending
        // registration as verified and opens a window for the name/password step.
        public async Task<RegisterResponse> VerifyRegistrationOtpAsync(string phoneNumber, string otp)
        {
            var pending = await _pendingRegistrationRepository.GetByPhoneNumberAsync(phoneNumber);
            if (pending == null || pending.Otp != otp || pending.OtpExpiry < DateTime.UtcNow)
            {
                throw new AppException(ErrorCodes.OtpInvalidOrExpired);
            }

            pending.IsOtpVerified = true;
            // Reuse OtpExpiry as the deadline for finishing the remaining step.
            pending.OtpExpiry = DateTime.UtcNow.Add(CompleteRegistrationWindow);
            await _pendingRegistrationRepository.UpdateAsync(pending);

            return new RegisterResponse(phoneNumber, _errorLocalizer.Localize(ErrorCodes.OtpVerified, _language.Language));
        }

        // Step 3: the phone is verified, so take the name + password and create the real User.
        // This is the only place a User row is created for a phone-based signup.
        public async Task<AuthResponse> CompleteRegistrationAsync(CompleteRegistrationRequest request)
        {
            var pending = await _pendingRegistrationRepository.GetByPhoneNumberAsync(request.PhoneNumber);
            if (pending == null)
            {
                throw AppException.NotFound(ErrorCodes.RegistrationNotFound);
            }

            if (!pending.IsOtpVerified || pending.OtpExpiry < DateTime.UtcNow)
            {
                throw new AppException(ErrorCodes.RegistrationOtpNotVerified);
            }

            // Guards against a concurrent signup (another device, a retried request) already
            // having created this account between verify and complete.
            var existingUser = await _userRepository.GetByPhoneNumberAsync(request.PhoneNumber);
            if (existingUser != null)
            {
                await _pendingRegistrationRepository.DeleteAsync(pending.Id);
                throw new AppException(ErrorCodes.PhoneAlreadyExists, 409);
            }

            var user = new User
            {
                Username = pending.PhoneNumber,
                PhoneNumber = pending.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                IsPhoneVerified = true
            };

            await _userRepository.AddAsync(user);
            await _pendingRegistrationRepository.DeleteAsync(pending.Id);

            var token = GenerateJwtToken(user);

            return new AuthResponse(token, user.Id, user.Username, user.Email, user.FullName, user.PhoneNumber, user.IsPhoneVerified, user.Role.ToString());
        }

        public async Task ResendRegistrationOtpAsync(string phoneNumber)
        {
            var pending = await _pendingRegistrationRepository.GetByPhoneNumberAsync(phoneNumber);
            if (pending == null)
            {
                throw AppException.NotFound(ErrorCodes.RegistrationNotFound);
            }

            var otp = GenerateOtp();

            pending.Otp = otp;
            pending.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
            // A fresh code has to be re-confirmed.
            pending.IsOtpVerified = false;

            await _pendingRegistrationRepository.UpdateAsync(pending);

            await DispatchOtpAsync(phoneNumber, otp);
        }

        public async Task UpdateFcmTokenAsync(Guid userId, string token)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.FcmToken = token;
                await _userRepository.UpdateAsync(user);
            }
        }

        public async Task DeleteAccountAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                throw AppException.NotFound(ErrorCodes.AccountNotFound);
            }

            await _userRepository.DeleteAsync(userId);
        }

        public async Task ForgotPasswordAsync(string phoneNumber)
        {
            var user = await _userRepository.GetByPhoneNumberAsync(phoneNumber);
            if (user == null)
            {
                throw AppException.NotFound(ErrorCodes.PhoneNotRegistered);
            }

            var otp = GenerateOtp();

            user.ResetPasswordOtp = otp;
            user.ResetPasswordOtpExpiry = DateTime.UtcNow.AddMinutes(10);
            
            await _userRepository.UpdateAsync(user);

            // Send OTP via WhatsApp (verify_code_1 template - one {{code}} parameter).
            await DispatchOtpAsync(phoneNumber, otp);
        }

        public async Task<bool> VerifyResetPasswordOtpAsync(string phoneNumber, string otp)
        {
            var user = await _userRepository.GetByPhoneNumberAsync(phoneNumber);
            if (user == null)
            {
                return false;
            }

            if (user.ResetPasswordOtp != otp || user.ResetPasswordOtpExpiry < DateTime.UtcNow)
            {
                return false;
            }

            return true;
        }

        public async Task ResetPasswordAsync(string phoneNumber, string otp, string newPassword)
        {
            var user = await _userRepository.GetByPhoneNumberAsync(phoneNumber);
            if (user == null)
            {
                throw AppException.NotFound(ErrorCodes.UserNotFound);
            }

            if (user.ResetPasswordOtp != otp || user.ResetPasswordOtpExpiry < DateTime.UtcNow)
            {
                throw new AppException(ErrorCodes.OtpInvalidOrExpired);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.ResetPasswordOtp = null;
            user.ResetPasswordOtpExpiry = null;

            await _userRepository.UpdateAsync(user);
        }

        // Test mode (Auth:UseTestOtp=true, set only in non-production configs): the OTP is a fixed,
        // known value and nothing is sent over WhatsApp. Missing key => false => real behaviour, so
        // production stays safe even if the section is absent.
        private bool UseTestOtp =>
            string.Equals(_configuration["Auth:UseTestOtp"], "true", StringComparison.OrdinalIgnoreCase);

        private string GenerateOtp()
        {
            return UseTestOtp
                ? (_configuration["Auth:TestOtpCode"] ?? "123456")
                : new Random().Next(100000, 999999).ToString();
        }

        // Delivers the OTP, or - in test mode - just logs it and skips WhatsApp entirely.
        private async Task DispatchOtpAsync(string phoneNumber, string otp)
        {
            if (UseTestOtp)
            {
                Console.WriteLine($"[TEST OTP] {phoneNumber} -> {otp}  (WhatsApp send skipped; Auth:UseTestOtp=true)");
                return;
            }

            await _whatsAppService.SendOtpAsync(phoneNumber, otp);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is missing");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim("username", user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            if (!string.IsNullOrEmpty(user.Email))
            {
                var claimList = claims.ToList();
                claimList.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
                claims = claimList.ToArray();
            }

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"] ?? "60")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
