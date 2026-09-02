using System;

namespace Ghasele.Application.DTOs
{
    /// Times are "HH:mm" strings on the wire — simplest to bind from an Angular
    /// <input type="time"> and from the mobile app, and avoids TimeOnly JSON quirks.

    public class DeliveryWindowDto
    {
        public Guid Id { get; set; }
        public string Start { get; set; } = "00:00";
        public string End { get; set; } = "00:00";
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateDeliveryWindowDto
    {
        public string Start { get; set; } = string.Empty;   // "13:00"
        public string End { get; set; } = string.Empty;     // "14:00"
        public int Capacity { get; set; } = 20;
    }

    public class UpdateDeliveryWindowDto
    {
        public string? Start { get; set; }
        public string? End { get; set; }
        public int? Capacity { get; set; }
        public bool? IsActive { get; set; }
    }

    /// A concrete bookable slot: one active window projected onto one calendar date.
    public class DeliverySlotDto
    {
        public Guid WindowId { get; set; }
        public DateOnly Date { get; set; }
        public string Start { get; set; } = string.Empty;
        public string End { get; set; } = string.Empty;
        public int Capacity { get; set; }
    }
}
