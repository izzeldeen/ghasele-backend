using System;

namespace Ghasele.Domain.Entities
{
    /// <summary>
    /// Single-row table holding operator-tunable values. Kept as typed columns rather
    /// than a key/value bag so money keeps its decimal precision and validation.
    /// Read and written through <see cref="Interfaces.IAppSettingsRepository"/>, which
    /// guarantees the row exists.
    /// </summary>
    public class AppSettings
    {
        /// <summary>Fixed id - there is only ever one row.</summary>
        public static readonly Guid SingletonId = Guid.Parse("a51e7c10-0000-4000-8000-000000000001");

        public Guid Id { get; set; } = SingletonId;

        /// <summary>Delivery fee charged on a Normal order, in JOD.</summary>
        public decimal NormalDeliveryPrice { get; set; }

        /// <summary>Delivery fee charged on an Express order, in JOD.</summary>
        public decimal ExpressDeliveryPrice { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
