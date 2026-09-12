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

        /// <summary>
        /// Random per-install identifier sent by the app, so a guest can see the orders they
        /// placed from this device. Null for signed-in orders, which are found by UserId.
        /// </summary>
        /// <remarks>
        /// This is the only credential a guest has, so whoever holds the value can read the
        /// orders under it. Acceptable because it is generated on the device, never displayed,
        /// and only ever exposes that device's own guest orders - but it does mean the value
        /// must stay out of URLs and access logs, which is why it travels in a header.
        /// </remarks>
        public string? DeviceToken { get; set; }

        /// <summary>
        /// FCM push token of the device that placed a guest order, so status updates can still
        /// reach the phone. Null for signed-in orders, which push to User.FcmToken instead.
        /// </summary>
        /// <remarks>
        /// Held on the order rather than looked up from a user row because a guest has no user
        /// row to hold it. FCM rotates these, so the app refreshes the value across this
        /// device's guest orders on every launch - see the guest fcm-token endpoint.
        /// </remarks>
        public string? FcmToken { get; set; }

        /// <summary>
        /// Where a push about this order should be sent, or null if there is nowhere to send it.
        /// </summary>
        /// <remarks>
        /// A guest order carries its own token because there is no user row to hold one; a
        /// signed-in order uses the account's, which stays current across devices. Reads
        /// <see cref="User"/>, so the caller must have loaded it for a signed-in order.
        /// </remarks>
        public string? ResolvePushToken() => UserId is null ? FcmToken : User?.FcmToken;

        public decimal TotalAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal DeliveryAmount { get; set; }
        public decimal CleanerAmount { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public OrderStatus Status { get; set; } = OrderStatus.PendingCollection;

        /// <summary>
        /// Retained so historical Express orders keep their meaning. Every new order is
        /// Normal: collection is scheduled into a delivery window now, and the customer is
        /// no longer offered an immediate-collection option to pay extra for.
        /// </summary>
        public OrderType Type { get; set; } = OrderType.Normal;

        /// <summary>
        /// The trip schedule slot the customer booked: which recurring window, on which date.
        /// </summary>
        /// <remarks>
        /// This is the reference to the operator's schedule - the window is the source of
        /// truth for which trip this order rides on. Nullable only because orders placed
        /// before scheduling existed have no slot; every new order requires one.
        /// </remarks>
        public Guid? DeliveryWindowId { get; set; }
        public DeliveryWindow? DeliveryWindow { get; set; }

        /// <summary>Local (Amman) date the order is scheduled for collection.</summary>
        public DateOnly? ScheduledDate { get; set; }

        /// <summary>
        /// The window's local start and end, copied from the delivery window at booking time.
        /// </summary>
        /// <remarks>
        /// Deliberately a snapshot rather than a read through <see cref="DeliveryWindow"/>.
        /// The window is a living row the operator can edit; if these were derived, moving
        /// a window from 10:00 to 14:00 would silently move every order already booked into
        /// it, and a customer would be told a different time than the one they agreed to.
        /// Editing a window therefore affects future bookings only.
        /// </remarks>
        public TimeOnly? ScheduledStartTime { get; set; }
        public TimeOnly? ScheduledEndTime { get; set; }

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
