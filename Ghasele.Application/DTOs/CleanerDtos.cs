using System;
using System.Collections.Generic;

namespace Ghasele.Application.DTOs
{
    public class CleanerDto
    {
        public Guid Id { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        /// <summary>NameAr or NameEn, chosen for the request's language - for callers that just want a name to display.</summary>
        public string Name { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string? CleaningLocation { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCleanerDto
    {
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string? CleaningLocation { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    /// <summary>
    /// One row of a cleaner's rate card: the customer prices for an item type alongside what
    /// this cleaner has agreed to be paid for it.
    /// </summary>
    /// <remarks>
    /// Both sides travel together because the admin screen sets the agreed rate while looking at
    /// what the customer pays for the same item. The customer figures are read-only here - they
    /// are edited on the Item Types screen and are the same for every cleaner.
    /// </remarks>
    public class CleanerItemPriceDto
    {
        public Guid ItemTypeId { get; set; }
        public string TypeNameAr { get; set; } = string.Empty;
        public string TypeNameEn { get; set; } = string.Empty;
        /// <summary>TypeNameAr or TypeNameEn, chosen for the request's language.</summary>
        public string TypeName { get; set; } = string.Empty;

        /// <summary>What the customer pays. From ItemType; not editable through this endpoint.</summary>
        public decimal CustomerIronPrice { get; set; }
        public decimal CustomerCleaningPrice { get; set; }
        public decimal CustomerBothPrice { get; set; }

        /// <summary>What this cleaner is paid. 0 means no rate has been agreed for that service.</summary>
        public decimal IronPrice { get; set; }
        public decimal CleaningPrice { get; set; }
        public decimal BothPrice { get; set; }

        /// <summary>False when this cleaner has no agreed rate for the item type at all.</summary>
        public bool HasAgreedPrice { get; set; }
    }

    public class SaveCleanerItemPriceDto
    {
        public Guid ItemTypeId { get; set; }
        public decimal IronPrice { get; set; }
        public decimal CleaningPrice { get; set; }
        public decimal BothPrice { get; set; }
    }

    /// <summary>
    /// The whole rate card for one cleaner. Item types left out of <see cref="Items"/> have their
    /// agreed rates removed, so the admin clearing a row actually clears it.
    /// </summary>
    public class SaveCleanerItemPricesDto
    {
        public List<SaveCleanerItemPriceDto> Items { get; set; } = new();
    }

    public class UpdateCleanerDto
    {
        public string? NameAr { get; set; }
        public string? NameEn { get; set; }
        public string? Note { get; set; }
        public string? CleaningLocation { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
