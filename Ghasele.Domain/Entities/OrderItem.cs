using System;

namespace Ghasele.Domain.Entities
{
    public class OrderItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ItemType { get; set; } = string.Empty; // e.g., Shirt, Pants, Dress
        public ServiceType ServiceType { get; set; } = ServiceType.Iron; // Default to Iron or whatever
        public int Quantity { get; set; }

        public Guid OrderId { get; set; }
        public Order? Order { get; set; }
    }
}
