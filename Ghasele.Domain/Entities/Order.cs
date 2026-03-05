using System;

namespace Ghasele.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public double Lat { get; set; }
        public double Long { get; set; }
        public Guid UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal DeliveryAmount { get; set; }
        public decimal CleanerAmount { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public User? User { get; set; }
        public Guid? TripId { get; set; }
        public Trip? Trip { get; set; }
        
        public List<OrderItem> Items { get; set; } = new();

        // Marketing fields
        public Guid? MarketingCodeId { get; set; }
        public MarketingCode? MarketingCode { get; set; }
        public decimal MarketingDiscount { get; set; }
        public decimal MarketerShare { get; set; }
        public decimal MarketingDiscountPercentage { get; set; }
        public decimal MarketerSharePercentage { get; set; }
    }
}
