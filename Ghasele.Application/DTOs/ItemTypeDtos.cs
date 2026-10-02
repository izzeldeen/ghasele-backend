using System;

namespace Ghasele.Application.DTOs
{
    public class CreateItemTypeDto
    {
        public string TypeNameAr { get; set; } = string.Empty;
        public string TypeNameEn { get; set; } = string.Empty;

        /// <summary>
        /// The least washing and ironing one of these can cost. There is one service, so
        /// one price.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// The ceiling. Null means the price is open-ended and shows as a starting price;
        /// equal to <see cref="Price"/> means a fixed price; above it means a range. Below
        /// it is rejected. See ItemType for the full rule.
        /// </summary>
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// Position in the customer-facing catalogue, lowest first. Optional: omit it and a
        /// new item is appended to the end rather than jumping to the front, which is what a
        /// plain 0 would do. Callers written before this field existed keep working.
        /// </summary>
        public int? SortOrder { get; set; }
    }

    public class ItemTypeDto
    {
        public Guid Id { get; set; }
        public string TypeNameAr { get; set; } = string.Empty;
        public string TypeNameEn { get; set; } = string.Empty;
        /// <summary>TypeNameAr or TypeNameEn, chosen for the request's language - for callers that just want a name to display.</summary>
        public string TypeName { get; set; } = string.Empty;

        /// <summary>The base price for washing and ironing.</summary>
        public decimal Price { get; set; }

        /// <summary>The ceiling, or null when the price is open-ended.</summary>
        public decimal? MaxPrice { get; set; }

        /// <summary>Position in the catalogue, lowest first. See ItemType.SortOrder.</summary>
        public int SortOrder { get; set; }
    }
}
