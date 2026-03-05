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
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
