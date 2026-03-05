using System;

namespace Ghasele.Application.DTOs
{
    public class CreateTicketDto
    {
        public string? UserId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Status { get; set; }
    }

    public class TicketDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Response { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        public string? UserName { get; set; }
        public string? UserPhoneNumber { get; set; }
    }
}
