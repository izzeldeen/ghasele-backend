using System;

namespace Ghasele.Application.DTOs
{
    public class MarketingCodeDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public decimal SharePercentage { get; set; }
        public string MarketerName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateMarketingCodeDto
    {
        public string Code { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public decimal SharePercentage { get; set; }
        public string MarketerName { get; set; } = string.Empty;
    }

    public class UpdateMarketingCodeDto
    {
        public decimal? DiscountPercentage { get; set; }
        public decimal? SharePercentage { get; set; }
        public string? MarketerName { get; set; }
        public bool? IsActive { get; set; }
    }
}
