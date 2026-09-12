using System;

namespace Ghasele.Domain.Entities
{
    public class OrderItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ItemType { get; set; } = string.Empty; // e.g., Shirt, Pants, Dress
        public ServiceType ServiceType { get; set; } = ServiceType.Iron; // Default to Iron or whatever
        public int Quantity { get; set; }

        /// <summary>
        /// The item type this line was priced against, or null if the driver typed a name that
        /// matched none. Recorded alongside the free-text name so the match made at collection
        /// time is not re-guessed later, and so a renamed item type stays traceable.
        /// </summary>
        public Guid? ItemTypeId { get; set; }

        /// <summary>
        /// What the customer was charged for one of these, in JOD, as it stood when the driver
        /// itemised the pickup.
        /// </summary>
        /// <remarks>
        /// Stamped on the line rather than read back from ItemType, because a later price change
        /// must not restate what an existing order came to. The order's TotalAmount is the
        /// authoritative figure; this is the breakdown behind it.
        /// </remarks>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// What the assigned cleaner is owed for one of these, in JOD, as agreed when the driver
        /// itemised the pickup.
        /// </summary>
        /// <remarks>
        /// Taken from that cleaner's agreed rate (see CleanerItemPrice), and 0 when no rate has
        /// been agreed with them for this item. Frozen here for the same reason as UnitPrice:
        /// renegotiating a rate must not change what past orders owed.
        /// </remarks>
        public decimal UnitCleanerPrice { get; set; }

        public Guid OrderId { get; set; }
        public Order? Order { get; set; }
    }
}
