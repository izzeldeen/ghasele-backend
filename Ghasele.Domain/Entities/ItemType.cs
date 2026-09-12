using System;

namespace Ghasele.Domain.Entities
{
    /// <summary>
    /// A kind of garment and what the customer pays to have it done.
    /// </summary>
    /// <remarks>
    /// Customer prices only. What we pay a laundry is negotiated per laundry and lives on
    /// <see cref="CleanerItemPrice"/> - it differs from one cleaner to the next, so a single
    /// cost here could never be the right number for all of them.
    /// </remarks>
    public class ItemType
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string TypeNameAr { get; set; } = string.Empty;
        public string TypeNameEn { get; set; } = string.Empty;

        // Ironing
        public decimal IronPrice { get; set; }

        // Cleaning (Washing)
        public decimal CleaningPrice { get; set; }

        // Both (Washing + Ironing)
        public decimal BothPrice { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
