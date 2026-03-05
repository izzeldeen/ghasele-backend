using System;

namespace Ghasele.Domain.Entities
{
    public class MarketingCode
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Code { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public decimal SharePercentage { get; set; }
        public string MarketerName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
