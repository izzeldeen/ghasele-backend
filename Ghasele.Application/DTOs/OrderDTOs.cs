using System;

namespace Ghasele.Application.DTOs
{
    /// <summary>
    /// Body of the guest FCM token refresh. The device itself is identified by the
    /// X-Device-Token header, not by anything in here.
    /// </summary>
    public class UpdateGuestFcmTokenDto
    {
        public string FcmToken { get; set; } = string.Empty;
    }

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

        /// <summary>
        /// Per-install identifier of the device placing a guest order, taken from the
        /// X-Device-Token header rather than the body - the header is what the guest order
        /// listing reads back, and accepting it in two places invites the two to disagree.
        /// </summary>
        public string? DeviceToken { get; set; }

        /// <summary>
        /// FCM push token of the device placing a guest order. Optional - an order is still
        /// placed without it, the customer just gets no push updates. Ignored for signed-in
        /// orders, which push to the token on the user row.
        /// </summary>
        public string? FcmToken { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal DeliveryAmount { get; set; }
        public decimal CleanerAmount { get; set; }
        public string? MarketingCode { get; set; }

        /// <summary>
        /// The trip schedule window the customer booked, from
        /// <c>GET /api/delivery-windows/slots</c>. Required.
        /// </summary>
        public Guid? DeliveryWindowId { get; set; }

        /// <summary>
        /// Local (Amman) date for the booked window, "yyyy-MM-dd". Required, and must be a
        /// date the slots endpoint actually offered - the pair is re-validated server side.
        /// </summary>
        public DateOnly? ScheduledDate { get; set; }

        // No Type: the customer chooses a scheduled window, not a delivery speed. The old
        // Normal/Express pair is gone, so a client sending one has nothing to select.
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

        /// <summary>The booked trip schedule window, or null for a pre-scheduling order.</summary>
        public Guid? DeliveryWindowId { get; set; }

        /// <summary>Local collection date, "yyyy-MM-dd".</summary>
        public DateOnly? ScheduledDate { get; set; }

        /// <summary>Local window bounds as "HH:mm", as agreed with the customer.</summary>
        public string? ScheduledStart { get; set; }
        public string? ScheduledEnd { get; set; }

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

        /// <summary>What the customer was charged per unit, as priced at collection time.</summary>
        public decimal UnitPrice { get; set; }

        /// <summary>What the assigned cleaner is owed per unit, as agreed at collection time.</summary>
        public decimal UnitCleanerPrice { get; set; }

        /// <summary>UnitPrice x Quantity - this line's share of the order's TotalAmount.</summary>
        public decimal LineTotal { get; set; }

        /// <summary>UnitCleanerPrice x Quantity - this line's share of the order's CleanerAmount.</summary>
        public decimal LineCleanerAmount { get; set; }
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
