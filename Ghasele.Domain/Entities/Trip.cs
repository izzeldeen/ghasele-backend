using System;
using System.Collections.Generic;

namespace Ghasele.Domain.Entities
{
    public class Trip
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ReferenceNumber { get; set; } = string.Empty;
        public TripStatus Status { get; set; } = TripStatus.Assigned;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? AssignedDriverId { get; set; }
        public Driver? Driver { get; set; }
        public Guid? CleanerId { get; set; }
        public Cleaner? Cleaner { get; set; }
        
        // Stores the optimized route as a JSON string
        public string? RouteJson { get; set; }

        public double? StartLocationLat { get; set; }
        public double? StartLocationLng { get; set; }

        /// <summary>
        /// Set the moment the captain taps "Start trip" in the driver app. Null means the trip
        /// is assigned but the captain has not left yet. Starting also (re)computes RouteJson
        /// from the captain's position if it is empty.
        /// </summary>
        public DateTime? StartedAt { get; set; }

        // Navigation collection
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
