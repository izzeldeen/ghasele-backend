using System;

namespace Ghasele.Application.DTOs
{
    public class CreateOrderDto
    {
        public double Lat { get; set; }
        public double Long { get; set; }
        public Guid UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal DeliveryAmount { get; set; }
        public decimal CleanerAmount { get; set; }
        public string? MarketingCode { get; set; }
    }

    public class UpdateOrderDto
    {
        public double? Lat { get; set; }
        public double? Long { get; set; }
        public decimal? CleanerAmount { get; set; }
        public string? Status { get; set; }
    }

    public class OrderDto
    {
        public Guid Id { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
        public Guid UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserPhoneNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal DeliveryAmount { get; set; }
        public decimal CleanerAmount { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid? TripId { get; set; }
        public string? TripReferenceNumber { get; set; }
        public string? CleanerName { get; set; }
        public string? DriverName { get; set; }
        public string? DriverPhoneNumber { get; set; }
        public string? MarketingCode { get; set; }
        public decimal MarketingDiscount { get; set; }
        public decimal MarketerShare { get; set; }
        public decimal MarketingDiscountPercentage { get; set; }
        public decimal MarketerSharePercentage { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public Guid Id { get; set; }
        public string ItemType { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public class AddOrderItemsDto
    {
        public List<AddOrderItemDto> Items { get; set; } = new();
    }

    public class AddOrderItemDto
    {
        public string ItemType { get; set; } = string.Empty;
        public string ServiceType { get; set; } = "Iron";
        public int Quantity { get; set; }
    }
}
