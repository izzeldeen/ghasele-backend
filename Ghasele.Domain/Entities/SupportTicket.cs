using System;

namespace Ghasele.Domain.Entities
{
    public class SupportTicket
    {
        public int Id { get; set; }
        public string UserId { get; set; }
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
