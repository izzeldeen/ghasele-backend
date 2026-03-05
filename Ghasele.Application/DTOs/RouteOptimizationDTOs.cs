using System.Collections.Generic;

namespace Ghasele.Application.DTOs
{
    public class OptimizeRouteRequestDto
    {
        public double StartLat { get; set; }
        public double StartLng { get; set; }
        public double? EndLat { get; set; }
        public double? EndLng { get; set; }
        public List<LocationDto> Locations { get; set; } = new();
    }

    public class LocationDto
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string? Id { get; set; } // Optional: To identify the location (e.g., OrderId)
    }

    public class OptimizedRouteResponseDto
    {
        public List<LocationDto> OptimizedRoute { get; set; } = new();
        public double TotalDistance { get; set; }
        public double TotalDuration { get; set; }
    }
}
