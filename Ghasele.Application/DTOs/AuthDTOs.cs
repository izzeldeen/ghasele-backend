using System;

namespace Ghasele.Application.DTOs
{
    public record RegisterRequest(string Password, string FullName, string PhoneNumber);
    
    public record LoginRequest(string PhoneNumber, string Password);

    // FullName/Email are only supplied by Apple on the very first sign-in; the
    // IdentityToken is always present and is the source of truth we verify.
    public record AppleSignInRequest(string IdentityToken, string? FullName, string? Email);
    
    public record AuthResponse(string Token, Guid Id, string Username, string? Email, string FullName, string PhoneNumber, bool IsPhoneVerified);

    public record UpdateFcmTokenRequest(Guid UserId, string Token);

    public record ForgotPasswordRequest(string PhoneNumber);
    public record VerifyOtpRequest(string PhoneNumber, string Otp);
    public record ResetPasswordRequest(string PhoneNumber, string Otp, string NewPassword);

    public record VerifyRegistrationOtpRequest(string PhoneNumber, string Otp);
    public record ResendRegistrationOtpRequest(string PhoneNumber);
}
