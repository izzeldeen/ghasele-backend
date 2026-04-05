using System;
using System.Collections.Generic;

namespace Ghasele.Application.DTOs
{
    public class CreateTripDto
    {
        public List<Guid> OrderIds { get; set; } = new List<Guid>();
        public string? RouteJson { get; set; }
        public double? StartLocationLat { get; set; }
        public double? StartLocationLng { get; set; }
        public Guid? CleanerId { get; set; }
        public Guid? DriverId { get; set; }
    }

    public class UpdateTripStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }

    public class UpdateTripCleanerDto
    {
        public Guid? CleanerId { get; set; }
    }

    public class UpdateTripDriverDto
    {
        public Guid? DriverId { get; set; }
    }

    public class AssignOrdersToTripDto
    {
        public Guid TripId { get; set; }
        public List<Guid> OrderIds { get; set; } = new List<Guid>();
        public string? RouteJson { get; set; }
        public double? StartLocationLat { get; set; }
        public double? StartLocationLng { get; set; }
    }

    public class TripDto
    {
        public Guid Id { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public Guid? CleanerId { get; set; }
        public string? CleanerName { get; set; }
        public Guid? DriverId { get; set; }
        public string? DriverName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? RouteJson { get; set; }
        public double? StartLocationLat { get; set; }
        public double? StartLocationLng { get; set; }
        public double? CleanerLat { get; set; }
        public double? CleanerLng { get; set; }
        public int OrderCount { get; set; }
        public List<OrderDto> Orders { get; set; } = new List<OrderDto>();
    }
}
