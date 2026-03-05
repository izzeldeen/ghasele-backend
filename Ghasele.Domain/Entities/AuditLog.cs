using System;

namespace Ghasele.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? UserId { get; set; }
        public string RequestPath { get; set; } = string.Empty;
        public string RequestMethod { get; set; } = string.Empty;
        public string ExceptionMessage { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
