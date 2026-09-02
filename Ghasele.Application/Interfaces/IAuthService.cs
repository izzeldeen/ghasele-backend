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
        Task UpdateFcmTokenAsync(Guid userId, string token);
        Task ForgotPasswordAsync(string phoneNumber);
        Task<bool> VerifyResetPasswordOtpAsync(string phoneNumber, string otp);
        Task ResetPasswordAsync(string phoneNumber, string otp, string newPassword);

        // Phone-first registration: request a code, confirm it, then create the account.
        Task<RegisterResponse> StartRegistrationAsync(string phoneNumber);
        Task<RegisterResponse> VerifyRegistrationOtpAsync(string phoneNumber, string otp);
        Task ResendRegistrationOtpAsync(string phoneNumber);
        Task<AuthResponse> CompleteRegistrationAsync(CompleteRegistrationRequest request);

        /// App Store guideline 5.1.1(v): an app that offers account creation must
        /// also let the user delete that account from inside the app.
        Task DeleteAccountAsync(Guid userId);
    }
}
