using System;

namespace Ghasele.Domain.Entities
{
    /// <summary>
    /// A recurring daily time window that deliveries are scheduled into, e.g. 13:00-14:00.
    /// The operator defines a handful of these in the dashboard; the customer app books a
    /// specific date against one of them, and dispatch builds a trip per (window, date).
    /// </summary>
    public class DeliveryWindow
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Local start of the window (no date component).</summary>
        public TimeOnly StartTime { get; set; }

        /// <summary>Local end of the window. Must be after <see cref="StartTime"/>.</summary>
        public TimeOnly EndTime { get; set; }

        /// <summary>Maximum number of orders that can be booked into this window on a single day.</summary>
        public int Capacity { get; set; } = 20;

        /// <summary>Inactive windows are hidden from the customer app but kept for history.</summary>
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
