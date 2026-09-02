using System;

namespace Ghasele.Application.DTOs
{
    /// Step 1 of the phone-first registration flow: the user supplies only a phone number and
    /// the backend sends a WhatsApp OTP.
    public record StartRegistrationRequest(string PhoneNumber);

    /// Step 3 of the phone-first registration flow: once the OTP is confirmed, the user picks a
    /// name and password and the account is created.
    public record CompleteRegistrationRequest(string PhoneNumber, string FullName, string Password);

    public record LoginRequest(string PhoneNumber, string Password);

    // FullName/Email are only supplied by Apple on the very first sign-in; the
    // IdentityToken is always present and is the source of truth we verify.
    public record AppleSignInRequest(string IdentityToken, string? FullName, string? Email);
    
    public record AuthResponse(string Token, Guid Id, string Username, string? Email, string FullName, string PhoneNumber, bool IsPhoneVerified, string Role);

    /// Returned by start-registration and OTP verification while the phone is still unverified:
    /// no user exists yet, so there is no token to hand back.
    public record RegisterResponse(string PhoneNumber, string Message);

    public record UpdateFcmTokenRequest(Guid UserId, string Token);

    public record ForgotPasswordRequest(string PhoneNumber);
    public record VerifyOtpRequest(string PhoneNumber, string Otp);
    public record ResetPasswordRequest(string PhoneNumber, string Otp, string NewPassword);

    public record VerifyRegistrationOtpRequest(string PhoneNumber, string Otp);
    public record ResendRegistrationOtpRequest(string PhoneNumber);
}
