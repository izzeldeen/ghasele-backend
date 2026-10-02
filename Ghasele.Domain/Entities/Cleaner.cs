using System;

namespace Ghasele.Domain.Entities
{
    public class Cleaner
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string? CleaningLocation { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        /// <summary>
        /// The share of each order line this laundry is paid, as a percentage of what the
        /// customer was charged. 50 means half; 60 means they keep more and we keep less.
        /// </summary>
        /// <remarks>
        /// One number per laundry replaces the per-item rate card that used to live on
        /// CleanerItemPrice. A rate agreed per garment could not survive prices being
        /// settled at the door: once the driver decides what a coat costs on this order,
        /// the only figure that can follow it is a share of it.
        /// <para>
        /// Stored as 0-100 rather than a 0-1 fraction because that is how the agreement is
        /// written and how an operator types it. Divided at the point of use, never before.
        /// </para>
        /// </remarks>
        public decimal SharePercentage { get; set; } = 50m;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
