using System;

namespace Ghasele.Domain.Entities
{
    /// <summary>
    /// What one cleaner has agreed to be paid for one item type, per service.
    /// </summary>
    /// <remarks>
    /// The customer-facing prices live on <see cref="ItemType"/> and are the same for everyone.
    /// What we pay is negotiated per laundry - the same shirt can cost 1.20 at one cleaner and
    /// 1.50 at another - so it cannot live on the item type. One row per (cleaner, item type)
    /// keeps that out of ItemType without duplicating item types themselves.
    ///
    /// These are the rates in force *now*. An order that has already been priced keeps the
    /// figures it was priced with on its own rows (see OrderItem.UnitCleanerPrice), so
    /// renegotiating with a cleaner never rewrites what a past order owed them.
    /// </remarks>
    public class CleanerItemPrice
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid CleanerId { get; set; }
        public Cleaner? Cleaner { get; set; }

        public Guid ItemTypeId { get; set; }
        public ItemType? ItemType { get; set; }

        /// <summary>Agreed rate for ironing one of these, in JOD.</summary>
        public decimal IronPrice { get; set; }

        /// <summary>Agreed rate for cleaning (washing) one of these, in JOD.</summary>
        public decimal CleaningPrice { get; set; }

        /// <summary>Agreed rate for washing and ironing one of these, in JOD.</summary>
        public decimal BothPrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// The agreed rate for one service, or 0 when this cleaner has no rate for it - an item
        /// they are simply not paid for. Item types carry customer prices only, so there is no
        /// other figure to reach for.
        /// </summary>
        public decimal PriceFor(ServiceType serviceType) => serviceType switch
        {
            ServiceType.Cleaning => CleaningPrice,
            ServiceType.Both => BothPrice,
            _ => IronPrice
        };
    }
}
