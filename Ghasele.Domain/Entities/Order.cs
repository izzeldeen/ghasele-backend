using System;

namespace Ghasele.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public double Lat { get; set; }
        public double Long { get; set; }
        /// <summary>
        /// Owner of the order, or null for a guest order placed without an account.
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// True when the order was placed without signing in. A separate column rather than
        /// inferred from UserId being null, so the distinction survives any later account linking
        /// and admin screens can filter on it directly.
        /// </summary>
        public bool IsGuest { get; set; }

        /// <summary>
        /// Phone number captured at checkout for a guest order, in E.164 (+962...).
        /// </summary>
        /// <remarks>
        /// A guest has no user row, so this is the only way a driver can reach them. Required for
        /// guest orders; null for signed-in ones, which carry the number on the user record.
        /// </remarks>
        public string? ContactPhoneNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal DeliveryAmount { get; set; }
        public decimal CleanerAmount { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public OrderStatus Status { get; set; } = OrderStatus.PendingCollection;

        /// <summary>
        /// Chosen by the customer at checkout. Drives which delivery fee is stamped on
        /// the order and how prominently it surfaces in the admin's trip-building screen.
        /// </summary>
        public OrderType Type { get; set; } = OrderType.Normal;

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
