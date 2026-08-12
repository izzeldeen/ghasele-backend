using System.Threading.Tasks;
using Ghasele.Application.DTOs;

namespace Ghasele.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> AppleSignInAsync(AppleSignInRequest request);
        Task UpdateFcmTokenAsync(Guid userId, string token);
        Task ForgotPasswordAsync(string phoneNumber);
        Task<bool> VerifyResetPasswordOtpAsync(string phoneNumber, string otp);
        Task ResetPasswordAsync(string phoneNumber, string otp, string newPassword);
        Task<bool> VerifyRegistrationOtpAsync(string phoneNumber, string otp);
        Task ResendRegistrationOtpAsync(string phoneNumber);

        /// App Store guideline 5.1.1(v): an app that offers account creation must
        /// also let the user delete that account from inside the app.
        Task DeleteAccountAsync(Guid userId);
    }
}
