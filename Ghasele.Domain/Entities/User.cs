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
        public UserRole Role { get; set; } = UserRole.Client;

        /// <summary>
        /// Whether this person may sign in to the admin dashboard.
        /// </summary>
        /// <remarks>
        /// Deliberately separate from <see cref="Role"/> rather than derived from it. Role
        /// says what someone does in the product - a customer, a captain - and a person can
        /// be exactly one of those. Portal access is a different question that cuts across
        /// it: an operations captain may need the dashboard without ceasing to be a captain,
        /// and a Client row must never gain it by accident.
        /// <para>
        /// Defaults to false, so an account created by any of the sign-up paths - app,
        /// Apple, Google, Firebase phone - has no dashboard access until someone grants it
        /// explicitly. That default is the whole point: every user could reach the portal
        /// before this existed.
        /// </para>
        /// </remarks>
        public bool IsAdmin { get; set; } = false;

        // Stable, per-app Apple user identifier (the "sub" claim from Apple's identity token).
        public string? AppleUserId { get; set; }

        public string? ResetPasswordOtp { get; set; }
        public DateTime? ResetPasswordOtpExpiry { get; set; }

        public bool IsPhoneVerified { get; set; } = false;
    }
}
