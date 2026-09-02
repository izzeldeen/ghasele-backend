using System;

namespace Ghasele.Domain.Entities
{
    public class ItemType
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string TypeNameAr { get; set; } = string.Empty;
        public string TypeNameEn { get; set; } = string.Empty;

        // Ironing
        public decimal IronPrice { get; set; }
        public decimal IronCost { get; set; }

        // Cleaning (Washing)
        public decimal CleaningPrice { get; set; }
        public decimal CleaningCost { get; set; }

        // Both (Washing + Ironing)
        public decimal BothPrice { get; set; }
        public decimal BothCost { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
