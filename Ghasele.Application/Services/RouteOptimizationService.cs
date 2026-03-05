using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;
using Ghasele.Application.Interfaces;

namespace Ghasele.Application.Services
{
    public class RouteOptimizationService : IRouteOptimizationService
    {
        // Simple Nearest Neighbor Algorithm for demonstration.
        // For production, consider using Google Maps Directions API or OSRM.
        public Task<OptimizedRouteResponseDto> OptimizeRouteAsync(OptimizeRouteRequestDto request)
        {
            var optimizedRoute = new List<LocationDto>();
            var remainingLocations = new List<LocationDto>(request.Locations);
            
            var currentLat = request.StartLat;
            var currentLng = request.StartLng;
            double totalDistance = 0;

            // Nearest Neighbor logic:
            // 1. Start from current location (Start Point)
            // 2. Find the closest unvisited location
            // 3. Move to that location
            // 4. Repeat until all visited

            while (remainingLocations.Count > 0)
            {
                LocationDto? nearest = null;
                double minDistance = double.MaxValue;

                foreach (var loc in remainingLocations)
                {
                    double distance = CalculateDistance(currentLat, currentLng, loc.Lat, loc.Lng);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearest = loc;
                    }
                }

                if (nearest != null)
                {
                    optimizedRoute.Add(nearest);
                    totalDistance += minDistance;
                    
                    // Update current position to the newly added location
                    currentLat = nearest.Lat;
                    currentLng = nearest.Lng;
                    
                    remainingLocations.Remove(nearest);
                }
            }

            // If an end point is specified, add it as the final destination
            if (request.EndLat.HasValue && request.EndLng.HasValue)
            {
                double distanceToEnd = CalculateDistance(currentLat, currentLng, request.EndLat.Value, request.EndLng.Value);
                totalDistance += distanceToEnd;
                
                optimizedRoute.Add(new LocationDto
                {
                    Lat = request.EndLat.Value,
                    Lng = request.EndLng.Value,
                    Id = "END_POINT"
                });
            }

            // Return simulated result. 
            // In a real scenario, you'd calculate duration based on traffic, etc.
            return Task.FromResult(new OptimizedRouteResponseDto
            {
                OptimizedRoute = optimizedRoute,
                TotalDistance = totalDistance,
                TotalDuration = totalDistance * 2 // Rough estimate: 2 mins per km
            });
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Haversine formula
            var R = 6371; // Radius of the earth in km
            var dLat = Deg2Rad(lat2 - lat1);
            var dLon = Deg2Rad(lon2 - lon1);
            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(Deg2Rad(lat1)) * Math.Cos(Deg2Rad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
                ;
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c; // Distance in km
            return d;
        }

        private double Deg2Rad(double deg)
        {
            return deg * (Math.PI / 180);
        }
    }
}
