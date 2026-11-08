using System;

namespace Ghasele.Application.DTOs
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>The order this notification is about, if any, so tapping it can deep-link there.</summary>
        public Guid? OrderId { get; set; }
    }
}
