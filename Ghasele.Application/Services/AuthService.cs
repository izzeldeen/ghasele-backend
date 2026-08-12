using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Ghasele.Application.DTOs;
using Ghasele.Application.Interfaces;
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
        private readonly IConfiguration _configuration;
        private readonly IWhatsAppService _whatsAppService;

        public AuthService(IUserRepository userRepository, IConfiguration configuration, IWhatsAppService whatsAppService)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _whatsAppService = whatsAppService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var existingPhone = await _userRepository.GetByPhoneNumberAsync(request.PhoneNumber);
            if (existingPhone != null)
            {
                throw new Exception("User with this phone number already exists.");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var otp = GenerateOtp();

            var user = new User
            {
                Username = request.PhoneNumber,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = passwordHash,
                FullName = request.FullName,
                IsPhoneVerified = false,
                RegistrationOtp = otp,
                RegistrationOtpExpiry = DateTime.UtcNow.AddMinutes(10)
            };

            await _userRepository.AddAsync(user);

            await _whatsAppService.SendMessageAsync(user.PhoneNumber, $"Your Ghasele verification code is: {otp}. This code expires in 10 minutes.");

            var token = GenerateJwtToken(user);

            return new AuthResponse(token, user.Id, user.Username, user.Email, user.FullName, user.PhoneNumber, user.IsPhoneVerified);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByPhoneNumberAsync(request.PhoneNumber);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new Exception("Invalid phone number or password.");
            }

            var token = GenerateJwtToken(user);

            return new AuthResponse(token, user.Id, user.Username, user.Email, user.FullName, user.PhoneNumber, user.IsPhoneVerified);
        }

        public async Task<AuthResponse> AppleSignInAsync(AppleSignInRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.IdentityToken))
            {
                throw new Exception("Missing Apple identity token.");
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

            return new AuthResponse(token, user.Id, user.Username, user.Email, user.FullName, user.PhoneNumber, user.IsPhoneVerified);
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
            ClaimsPrincipal principal;
            try
            {
                principal = handler.ValidateToken(identityToken, validationParameters, out _);
            }
            catch (Exception ex)
            {
                throw new Exception($"Invalid Apple identity token: {ex.Message}");
            }

            var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(sub))
            {
                throw new Exception("Apple identity token has no subject.");
            }

            var email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            return (sub, email);
        }

        public async Task<bool> VerifyRegistrationOtpAsync(string phoneNumber, string otp)
        {
            var user = await _userRepository.GetByPhoneNumberAsync(phoneNumber);
            if (user == null)
            {
                return false;
            }

            if (user.RegistrationOtp != otp || user.RegistrationOtpExpiry < DateTime.UtcNow)
            {
                return false;
            }

            user.IsPhoneVerified = true;
            user.RegistrationOtp = null;
            user.RegistrationOtpExpiry = null;

            await _userRepository.UpdateAsync(user);

            return true;
        }

        public async Task ResendRegistrationOtpAsync(string phoneNumber)
        {
            var user = await _userRepository.GetByPhoneNumberAsync(phoneNumber);
            if (user == null)
            {
                throw new Exception("User with this phone number does not exist.");
            }

            if (user.IsPhoneVerified)
            {
                throw new Exception("Phone number is already verified.");
            }

            var otp = GenerateOtp();

            user.RegistrationOtp = otp;
            user.RegistrationOtpExpiry = DateTime.UtcNow.AddMinutes(10);

            await _userRepository.UpdateAsync(user);

            await _whatsAppService.SendMessageAsync(phoneNumber, $"Your Ghasele verification code is: {otp}. This code expires in 10 minutes.");
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
                throw new Exception("Account not found.");
            }

            await _userRepository.DeleteAsync(userId);
        }

        public async Task ForgotPasswordAsync(string phoneNumber)
        {
            var user = await _userRepository.GetByPhoneNumberAsync(phoneNumber);
            if (user == null)
            {
                throw new Exception("User with this phone number does not exist.");
            }

            var otp = GenerateOtp();

            user.ResetPasswordOtp = otp;
            user.ResetPasswordOtpExpiry = DateTime.UtcNow.AddMinutes(10);
            
            await _userRepository.UpdateAsync(user);

            // Send OTP via WhatsApp
            await _whatsAppService.SendMessageAsync(phoneNumber, $"Your Ghasele password reset code is: {otp}. This code expires in 10 minutes.");
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
                throw new Exception("User not found.");
            }

            if (user.ResetPasswordOtp != otp || user.ResetPasswordOtpExpiry < DateTime.UtcNow)
            {
                throw new Exception("Invalid or expired OTP.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.ResetPasswordOtp = null;
            user.ResetPasswordOtpExpiry = null;

            await _userRepository.UpdateAsync(user);
        }

        private static string GenerateOtp()
        {
            return new Random().Next(100000, 999999).ToString();
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
                new Claim("username", user.Username)
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
