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

    /// <summary>Metadata for a photo the customer attached to a new ticket.</summary>
    public class TicketAttachmentUpload
    {
        public System.IO.Stream Content { get; set; } = System.IO.Stream.Null;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Length { get; set; }
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
        public string? AttachmentUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        public string? UserName { get; set; }
        public string? UserPhoneNumber { get; set; }
    }
}
