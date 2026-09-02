using System;

namespace Ghasele.Domain.Entities
{
    public class Driver
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid? UserId { get; set; }
        public User? User { get; set; }
    }
}
