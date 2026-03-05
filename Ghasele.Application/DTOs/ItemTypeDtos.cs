using System;

namespace Ghasele.Application.DTOs
{
    public class CreateItemTypeDto
    {
        public string TypeName { get; set; } = string.Empty;
        public decimal IronPrice { get; set; }
        public decimal IronCost { get; set; }
        public decimal CleaningPrice { get; set; }
        public decimal CleaningCost { get; set; }
        public decimal BothPrice { get; set; }
        public decimal BothCost { get; set; }
    }

    public class ItemTypeDto
    {
        public Guid Id { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public decimal IronPrice { get; set; }
        public decimal IronCost { get; set; }
        public decimal CleaningPrice { get; set; }
        public decimal CleaningCost { get; set; }
        public decimal BothPrice { get; set; }
        public decimal BothCost { get; set; }
    }
}
