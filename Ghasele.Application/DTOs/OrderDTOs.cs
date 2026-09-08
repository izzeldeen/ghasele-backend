using System;

namespace Ghasele.Application.DTOs
{
    public class CreateOrderDto
    {
        public double Lat { get; set; }
        public double Long { get; set; }
        /// <summary>Owner of the order. Null for a guest checkout.</summary>
        /// <remarks>
        /// Ignored when the request carries no bearer token - the controller derives the owner
        /// from the token instead, so an anonymous caller cannot attach an order to someone
        /// else's account by passing their id.
        /// </remarks>
        public Guid? UserId { get; set; }

        /// <summary>Contact number for a guest checkout, E.164. Required when UserId is null.</summary>
        public string? ContactPhoneNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal DeliveryAmount { get; set; }
        public decimal CleanerAmount { get; set; }
        public string? MarketingCode { get; set; }

        /// <summary>"Normal" or "Express". Anything else (or absent) falls back to Normal.</summary>
        public string? Type { get; set; }
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
        /// <summary>Null for guest orders.</summary>
        public Guid? UserId { get; set; }

        /// <summary>True when the order was placed without an account.</summary>
        public bool IsGuest { get; set; }

        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        /// <summary>
        /// Number to reach the customer on: the user record's for a signed-in order, the
        /// order's own ContactPhoneNumber for a guest one. Drivers read this field either way.
        /// </summary>
        public string UserPhoneNumber { get; set; } = string.Empty;
        public string? UserLocationName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal DeliveryAmount { get; set; }
        public decimal CleanerAmount { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid? TripId { get; set; }
        public string? TripReferenceNumber { get; set; }
        public Guid? CleanerId { get; set; }
        public string? CleanerName { get; set; }
        public double? CleanerLat { get; set; }
        public double? CleanerLng { get; set; }
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
