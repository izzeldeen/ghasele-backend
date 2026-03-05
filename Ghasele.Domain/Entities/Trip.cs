using System;
using System.Collections.Generic;

namespace Ghasele.Domain.Entities
{
    public class Trip
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ReferenceNumber { get; set; } = string.Empty;
        public TripStatus Status { get; set; } = TripStatus.Created;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? AssignedDriverId { get; set; }
        public Driver? Driver { get; set; }
        public Guid? CleanerId { get; set; }
        public Cleaner? Cleaner { get; set; }
        
        // Stores the optimized route as a JSON string
        public string? RouteJson { get; set; }

        public double? StartLocationLat { get; set; }
        public double? StartLocationLng { get; set; }
        
        // Navigation collection
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
