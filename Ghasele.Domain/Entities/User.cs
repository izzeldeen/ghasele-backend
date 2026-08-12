using System;

namespace Ghasele.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? FcmToken { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Stable, per-app Apple user identifier (the "sub" claim from Apple's identity token).
        public string? AppleUserId { get; set; }

        public string? ResetPasswordOtp { get; set; }
        public DateTime? ResetPasswordOtpExpiry { get; set; }

        public bool IsPhoneVerified { get; set; } = false;
        public string? RegistrationOtp { get; set; }
        public DateTime? RegistrationOtpExpiry { get; set; }
    }
}
