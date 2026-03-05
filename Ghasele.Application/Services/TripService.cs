using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;
using Ghasele.Application.Interfaces;
using Ghasele.Domain.Entities;
using Ghasele.Domain.Interfaces;

namespace Ghasele.Application.Services
{
    public class TripService : ITripService
    {
        private readonly ITripRepository _tripRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly INotificationService _notificationService;
        private readonly IUserNotificationService _userNotificationService;

        public TripService(ITripRepository tripRepository, IOrderRepository orderRepository, INotificationService notificationService, IUserNotificationService userNotificationService)
        {
            _tripRepository = tripRepository;
            _orderRepository = orderRepository;
            _notificationService = notificationService;
            _userNotificationService = userNotificationService;
        }

        public async Task<TripDto> CreateTripAsync(CreateTripDto dto)
        {
            var trip = new Trip
            {
                ReferenceNumber = $"TRP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper()}",
                Status = TripStatus.Created,
                CreatedAt = DateTime.UtcNow,
                RouteJson = dto.RouteJson,
                StartLocationLat = dto.StartLocationLat,
                StartLocationLng = dto.StartLocationLng,
                CleanerId = dto.CleanerId,
                AssignedDriverId = dto.DriverId
            };

            foreach (var orderId in dto.OrderIds)
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order != null)
                {
                    order.Status = OrderStatus.InProgress;
                    trip.Orders.Add(order);
                }
            }

            await _tripRepository.AddAsync(trip);

            // Send notifications and save to DB
            foreach (var order in trip.Orders)
            {
                if (order.User != null)
                {
                    string title = "تحديث طلب";
                    string body = "السائق في الطريق لاستلام طلبك!";
                    
                    await _userNotificationService.CreateNotificationAsync(order.UserId, title, body);

                    if (!string.IsNullOrEmpty(order.User.FcmToken))
                    {
                        await _notificationService.SendNotificationAsync(order.User.FcmToken, title, body);
                    }
                }
            }

            return MapToDto(trip);
        }

        public async Task<TripDto> AssignOrdersToTripAsync(AssignOrdersToTripDto dto)
        {
            var trip = await _tripRepository.GetByIdAsync(dto.TripId);
            if (trip == null) throw new Exception("Trip not found");

            foreach (var orderId in dto.OrderIds)
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order != null)
                {
                    order.Status = OrderStatus.InProgress;
                    if (!trip.Orders.Any(o => o.Id == orderId))
                    {
                        trip.Orders.Add(order);
                    }
                }
            }

            await _tripRepository.UpdateAsync(trip);

            // Send notifications
            foreach (var orderId in dto.OrderIds)
            {
                var order = trip.Orders.FirstOrDefault(o => o.Id == orderId);
                if (order?.User != null)
                {
                    string title = "تحديث رحلة";
                    string body = "تم إضافة طلبك إلى رحلة جديدة!";
                    
                    await _userNotificationService.CreateNotificationAsync(order.UserId, title, body);

                    if (!string.IsNullOrEmpty(order.User.FcmToken))
                    {
                        await _notificationService.SendNotificationAsync(order.User.FcmToken, title, body);
                    }
                }
            }

            return MapToDto(trip);
        }

        public async Task<TripDto> UpdateTripStatusAsync(Guid id, UpdateTripStatusDto dto)
        {
            var trip = await _tripRepository.GetByIdAsync(id);
            if (trip == null) throw new Exception("Trip not found");

            if (Enum.TryParse<TripStatus>(dto.Status, true, out var status))
            {
                trip.Status = status;

                // Assigned: Driver delivered to cleaner -> Orders are Collected
                if (status == TripStatus.Assigned)
                {
                    if (trip.CleanerId == null)
                    {
                        throw new InvalidOperationException($"Cannot move trip #{trip.ReferenceNumber} to Assigned. A cleaner must be selected first.");
                    }

                    // Ensure items are recorded before marking as Collected at cleaner
                    var ordersWithoutItems = trip.Orders.Where(o => o.Items == null || !o.Items.Any()).ToList();
                    if (ordersWithoutItems.Any())
                    {
                        var orderRefs = string.Join(", ", ordersWithoutItems.Select(o => o.ReferenceNumber));
                        throw new InvalidOperationException($"Cannot finalize collection at cleaner. Order(s) {orderRefs} have no items recorded.");
                    }

                    foreach (var order in trip.Orders)
                    {
                        order.Status = OrderStatus.Collected;
                    }
                }
                // Delivered: Driver finished delivering -> Orders are Delivered
                else if (status == TripStatus.Delivered)
                {
                    foreach (var order in trip.Orders)
                    {
                        order.Status = OrderStatus.Delivered;
                    }
                }
                // Delivering: Driver starting delivery journey
                else if (status == TripStatus.Delivering)
                {
                    foreach (var order in trip.Orders)
                    {
                         // Maintain state or mark as Shipped/Delivering if such status existed, for now they stay Collected or move to InProgress
                         // No specific order status change requested for start of delivery, but InProgress is safe.
                    }
                }

                await _tripRepository.UpdateAsync(trip);
            }
            else
            {
                 throw new Exception("Invalid status");
            }

            return MapToDto(trip);
        }

        public async Task<TripDto> UpdateTripCleanerAsync(Guid id, UpdateTripCleanerDto dto)
        {
            var trip = await _tripRepository.GetByIdAsync(id);
            if (trip == null) throw new Exception("Trip not found");

            trip.CleanerId = dto.CleanerId;
            
            await _tripRepository.UpdateAsync(trip);
            var updatedTrip = await _tripRepository.GetByIdAsync(id); 
            return MapToDto(updatedTrip!); 
        }

        public async Task<TripDto> UpdateTripDriverAsync(Guid id, UpdateTripDriverDto dto)
        {
            var trip = await _tripRepository.GetByIdAsync(id);
            if (trip == null) throw new Exception("Trip not found");

            trip.AssignedDriverId = dto.DriverId;
            
            await _tripRepository.UpdateAsync(trip);
            var updatedTrip = await _tripRepository.GetByIdAsync(id); 
            return MapToDto(updatedTrip!); 
        }

        public async Task<List<TripDto>> GetAllTripsAsync()
        {
            var trips = await _tripRepository.GetAllAsync();
            return trips.Select(MapToDto).ToList();
        }

        public async Task<TripDto?> GetTripByIdAsync(Guid id)
        {
            var trip = await _tripRepository.GetByIdAsync(id);
            return trip != null ? MapToDto(trip) : null;
        }

        private static TripDto MapToDto(Trip trip)
        {
            return new TripDto
            {
                Id = trip.Id,
                ReferenceNumber = trip.ReferenceNumber,
                Status = trip.Status.ToString(),
                CleanerId = trip.CleanerId,
                CleanerName = trip.Cleaner?.Name,
                DriverId = trip.AssignedDriverId,
                DriverName = trip.Driver?.Name,
                CreatedAt = trip.CreatedAt,
                RouteJson = trip.RouteJson,
                StartLocationLat = trip.StartLocationLat,
                StartLocationLng = trip.StartLocationLng,
                OrderCount = trip.Orders.Count,
                Orders = trip.Orders.Select(OrderService.MapToDto).ToList()
            };
        }
    }
}
