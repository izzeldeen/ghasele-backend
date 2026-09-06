using System.Threading.Tasks;
using Ghasele.Application.DTOs;

namespace Ghasele.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> AppleSignInAsync(AppleSignInRequest request);

        /// <summary>
        /// Signs a user in from a verified Firebase Phone Auth ID token, creating the account on
        /// first use. Independent of the WhatsApp OTP flow below, which remains the fallback.
        /// </summary>
        Task<AuthResponse> FirebaseLoginAsync(FirebaseLoginRequest request);
        Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest request);
        Task<AuthResponse> FirebaseCompleteRegistrationAsync(FirebaseCompleteRegistrationRequest request);
        Task UpdateFcmTokenAsync(Guid userId, string token);
        Task ForgotPasswordAsync(string phoneNumber);
        Task<bool> VerifyResetPasswordOtpAsync(string phoneNumber, string otp);
        Task ResetPasswordAsync(string phoneNumber, string otp, string newPassword);

        // Phone-first registration: request a code, confirm it, then create the account.
        Task<RegisterResponse> StartRegistrationAsync(string phoneNumber);

        /// <summary>
        /// Whether <paramref name="phoneNumber"/> already has a usable account, i.e. one with a
        /// password set. Called before an OTP is dispatched so a duplicate is caught on the first
        /// screen rather than after the user has received an SMS and chosen a name and password.
        /// </summary>
        /// <remarks>
        /// An account with an empty password hash reports false on purpose: those are half-finished
        /// registrations that <see cref="FirebaseCompleteRegistrationAsync"/> is meant to repair,
        /// and reporting them as taken would lock their owner out of finishing.
        /// </remarks>
        Task<bool> IsPhoneRegisteredAsync(string phoneNumber);
        Task<RegisterResponse> VerifyRegistrationOtpAsync(string phoneNumber, string otp);
        Task ResendRegistrationOtpAsync(string phoneNumber);
        Task<AuthResponse> CompleteRegistrationAsync(CompleteRegistrationRequest request);

        /// App Store guideline 5.1.1(v): an app that offers account creation must
        /// also let the user delete that account from inside the app.
        Task DeleteAccountAsync(Guid userId);
    }
}
