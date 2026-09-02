using System;

namespace Ghasele.Domain.Entities
{
    /// <summary>
    /// A signup that has not yet finished the phone-first registration flow. Held here instead of in
    /// <see cref="User"/> so an unverified phone number never occupies a real account.
    ///
    /// The row is created when the user asks for a code (phone only), flipped to
    /// <see cref="IsOtpVerified"/> once the WhatsApp OTP is confirmed, and finally deleted and
    /// replaced by a verified <see cref="User"/> when the user submits their name and password.
    /// </summary>
    public class PendingRegistration
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string PhoneNumber { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public DateTime OtpExpiry { get; set; }

        /// <summary>Set once the OTP has been confirmed; the name/password step requires it.</summary>
        public bool IsOtpVerified { get; set; }

        /// <summary>
        /// Filled in only by the final "complete registration" step. Null while the signup is still
        /// at the phone/OTP stage.
        /// </summary>
        public string? PasswordHash { get; set; }
        public string? FullName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
