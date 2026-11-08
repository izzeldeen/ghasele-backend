using System;

namespace Ghasele.Domain.Entities
{
    /// <summary>
    /// An entry in the customer's in-app notification list.
    /// </summary>
    /// <remarks>
    /// Owned by exactly one of <see cref="UserId"/> (a signed-in customer) or
    /// <see cref="DeviceToken"/> (a guest, identified by the install that placed the order).
    /// A guest's entries move onto their account when they sign in on that device, the same
    /// way their orders do.
    /// </remarks>
    public class Notification
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? UserId { get; set; }
        public User? User { get; set; }

        /// <summary>The guest install this belongs to, or null when it belongs to an account.</summary>
        public string? DeviceToken { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>The order this notification is about, if any, so tapping it can deep-link there.</summary>
        public Guid? OrderId { get; set; }
    }
}
