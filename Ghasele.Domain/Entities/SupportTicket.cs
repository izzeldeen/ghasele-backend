using System;

namespace Ghasele.Domain.Entities
{
    public class SupportTicket
    {
        public int Id { get; set; }

        /// <summary>
        /// Owner of the ticket, or null when a guest opened it without an account.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Random per-install identifier of the device that opened a guest ticket, so the guest
        /// can come back and read the reply. Null for tickets opened by a signed-in user.
        /// </summary>
        /// <remarks>Same trade-off as <see cref="Order.DeviceToken"/>; see the note there.</remarks>
        public string? DeviceToken { get; set; }

        /// <summary>
        /// Number to reach a guest on, in E.164 (+962...). Required for guest tickets - with no
        /// user row behind them, support would otherwise have no way to answer off-app.
        /// </summary>
        public string? ContactPhoneNumber { get; set; }

        public string Subject { get; set; }
        public string Message { get; set; }
        public string Category { get; set; } // General, Order Issue, etc.
        public string Status { get; set; } = "Open"; // Open, In Progress, Resolved, Closed
        public string? Response { get; set; }

        /// <summary>
        /// Root-relative URL of a single photo the customer attached when opening the
        /// ticket, e.g. "/uploads/support/ab12….jpg". Null when no photo was sent.
        /// </summary>
        public string? AttachmentUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
