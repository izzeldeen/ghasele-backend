using System;

namespace Ghasele.Application.DTOs
{
    public class CreateItemTypeDto
    {
        public string TypeNameAr { get; set; } = string.Empty;
        public string TypeNameEn { get; set; } = string.Empty;
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
        public string TypeNameAr { get; set; } = string.Empty;
        public string TypeNameEn { get; set; } = string.Empty;
        /// <summary>TypeNameAr or TypeNameEn, chosen for the request's language - for callers that just want a name to display.</summary>
        public string TypeName { get; set; } = string.Empty;
        public decimal IronPrice { get; set; }
        public decimal IronCost { get; set; }
        public decimal CleaningPrice { get; set; }
        public decimal CleaningCost { get; set; }
        public decimal BothPrice { get; set; }
        public decimal BothCost { get; set; }
    }
}
