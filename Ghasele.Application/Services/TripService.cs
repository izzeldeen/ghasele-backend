using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;
using Ghasele.Application.Exceptions;
using Ghasele.Application.Interfaces;
using Ghasele.Application.Localization;
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
        private readonly IRouteOptimizationService _routeOptimization;
        private readonly ICurrentLanguageProvider _language;

        public TripService(ITripRepository tripRepository, IOrderRepository orderRepository, INotificationService notificationService, IUserNotificationService userNotificationService, IRouteOptimizationService routeOptimization, ICurrentLanguageProvider language)
        {
            _tripRepository = tripRepository;
            _orderRepository = orderRepository;
            _notificationService = notificationService;
            _userNotificationService = userNotificationService;
            _routeOptimization = routeOptimization;
            _language = language;
        }

        /// <summary>
        /// Called when the captain taps "Start trip". Stamps StartedAt, records the captain's
        /// current position, and - if the trip has no optimized route yet - builds one from that
        /// position through every order stop and stores it as RouteJson (a JSON array of
        /// { lat, lng, id } where id is the OrderId, exactly the shape the admin dashboard writes).
        /// </summary>
        public async Task<TripDto> StartTripAsync(Guid tripId, double? startLat, double? startLng)
        {
            var trip = await _tripRepository.GetByIdAsync(tripId)
                       ?? throw AppException.NotFound(ErrorCodes.TripNotFound);

            if (trip.StartedAt == null)
            {
                trip.StartedAt = DateTime.UtcNow;
            }

            if (startLat.HasValue && startLng.HasValue)
            {
                trip.StartLocationLat = startLat;
                trip.StartLocationLng = startLng;
            }

            var needsRoute = string.IsNullOrWhiteSpace(trip.RouteJson);
            if (needsRoute && trip.StartLocationLat.HasValue && trip.StartLocationLng.HasValue)
            {
                var stops = trip.Orders
                    .Where(o => o.Lat != 0 || o.Long != 0)
                    .Select(o => new LocationDto { Lat = o.Lat, Lng = o.Long, Id = o.Id.ToString() })
                    .ToList();

                if (stops.Count > 0)
                {
                    var optimized = await _routeOptimization.OptimizeRouteAsync(new OptimizeRouteRequestDto
                    {
                        StartLat = trip.StartLocationLat.Value,
                        StartLng = trip.StartLocationLng.Value,
                        // For the collecting leg the round ends at the cleaner; for delivery it ends
                        // at the last drop. Passing the cleaner is harmless either way - it is only a
                        // hint to the nearest-neighbour ordering.
                        EndLat = trip.Cleaner?.Latitude,
                        EndLng = trip.Cleaner?.Longitude,
                        Locations = stops
                    });

                    // camelCase so the shape matches what the admin dashboard writes
                    // (JSON.stringify of { lat, lng, id }) and what both clients read.
                    trip.RouteJson = JsonSerializer.Serialize(optimized.OptimizedRoute, RouteJsonOptions);
                }
            }

            await _tripRepository.UpdateAsync(trip);
            return MapToDto(trip);
        }

        private static readonly JsonSerializerOptions RouteJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public async Task<TripDto> CreateTripAsync(CreateTripDto dto)
        {
            if (dto.CleanerId == null || dto.DriverId == null)
            {
                throw new AppException(ErrorCodes.TripCleanerAndDriverRequired);
            }

            if (dto.OrderIds == null || !dto.OrderIds.Any())
            {
                throw new AppException(ErrorCodes.TripOrdersRequired);
            }

            var trip = new Trip
            {
                ReferenceNumber = $"TRP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper()}",
                Status = TripStatus.Assigned,
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
                    if (order.TripId != null)
                    {
                        throw new AppException(ErrorCodes.TripOrderAlreadyAssigned, 400, order.ReferenceNumber);
                    }
                    order.Status = OrderStatus.Assigned;
                    trip.Orders.Add(order);
                }
            }

            await _tripRepository.AddAsync(trip);

            // Notifications
            foreach (var order in trip.Orders)
            {
                string title = "تحديث طلب";
                string body = "السائق في الطريق لاستلام طلبك!";

                // Only a signed-in order gets an in-app record - that list is keyed by user, and
                // a guest has no account to read it from. They still get the push below.
                if (order.UserId is Guid ownerId)
                {
                    await _userNotificationService.CreateNotificationAsync(ownerId, title, body);
                }

                // A guest order carries the device's push token itself; see Order.ResolvePushToken.
                var pushToken = order.ResolvePushToken();
                if (!string.IsNullOrEmpty(pushToken))
                {
                    await _notificationService.SendNotificationAsync(pushToken, title, body);
                }
            }

            return MapToDto(trip);
        }

        public async Task<TripDto> CollectOrderAsync(Guid orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) throw AppException.NotFound(ErrorCodes.OrderNotFound);
            if (order.TripId == null) throw new AppException(ErrorCodes.OrderNotPartOfTrip);

            var trip = await _tripRepository.GetByIdAsync(order.TripId.Value);
            if (trip == null) throw AppException.NotFound(ErrorCodes.TripNotFound);

            // Check if this is the "next" order to be collected (sequential logic)
            // For now, we allow any order in PendingCollection, but the UI should enforce sequence.
            // If we strictly want to enforce here:
            // var previousIncomplete = trip.Orders.Where(o => o.Status == OrderStatus.PendingCollection && o.CreatedAt < order.CreatedAt).Any();
            // if (previousIncomplete) throw new Exception("Orders must be collected one by one in sequence.");

            order.Status = OrderStatus.Collected;
            await _orderRepository.UpdateAsync(order);

            // Auto-transition trip status if all orders are Collected
            if (trip.Orders.All(o => o.Status == OrderStatus.Collected))
            {
                trip.Status = TripStatus.Collected;
                await _tripRepository.UpdateAsync(trip);
            }

            return MapToDto(trip);
        }

        public async Task<TripDto> DeliverToCleanerAsync(Guid tripId)
        {
            var trip = await _tripRepository.GetByIdAsync(tripId);
            if (trip == null) throw AppException.NotFound(ErrorCodes.TripNotFound);

            if (trip.Status != TripStatus.Collected)
            {
                throw new AppException(ErrorCodes.TripMustBeCollected);
            }

            trip.Status = TripStatus.Cleaning;
            foreach (var order in trip.Orders)
            {
                order.Status = OrderStatus.Cleaning;
            }

            await _tripRepository.UpdateAsync(trip);
            return MapToDto(trip);
        }

        public async Task<TripDto> UpdateOrderInTripStatusAsync(Guid orderId, Ghasele.Domain.Entities.OrderStatus status)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) throw AppException.NotFound(ErrorCodes.OrderNotFound);
            if (order.TripId == null) throw new AppException(ErrorCodes.OrderNotPartOfTrip);

            order.Status = status;
            await _orderRepository.UpdateAsync(order);

            var trip = await _tripRepository.GetByIdAsync(order.TripId.Value);
            if (trip == null) throw AppException.NotFound(ErrorCodes.TripNotFound);

            // Side Effects
            if (status == OrderStatus.OutForDelivery)
            {
                string title = "تحديث طلب";
                string body = "طلبك الآن في طريقه إليك!";

                // In-app record for accounts only, push for everyone - as on collection above.
                if (order.UserId is Guid ownerId)
                {
                    await _userNotificationService.CreateNotificationAsync(ownerId, title, body);
                }

                var pushToken = order.ResolvePushToken();
                if (!string.IsNullOrEmpty(pushToken))
                {
                    await _notificationService.SendNotificationAsync(pushToken, title, body);
                }
            }
            else if (status == OrderStatus.Delivered)
            {
                // Auto-transition trip status if all orders are Delivered
                if (trip.Orders.All(o => o.Status == OrderStatus.Delivered))
                {
                    trip.Status = TripStatus.Delivered;
                    await _tripRepository.UpdateAsync(trip);
                }
            }

            return MapToDto(trip);
        }

        public async Task<TripDto> DeliverOrderAsync(Guid orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) throw AppException.NotFound(ErrorCodes.OrderNotFound);
            if (order.TripId == null) throw new AppException(ErrorCodes.OrderNotPartOfTrip);

            var trip = await _tripRepository.GetByIdAsync(order.TripId.Value);
            if (trip == null) throw AppException.NotFound(ErrorCodes.TripNotFound);

            order.Status = OrderStatus.Delivered;
            await _orderRepository.UpdateAsync(order);

            // Auto-transition trip status if all orders are Delivered
            if (trip.Orders.All(o => o.Status == OrderStatus.Delivered))
            {
                trip.Status = TripStatus.Delivered;
                await _tripRepository.UpdateAsync(trip);
            }

            return MapToDto(trip);
        }

        public async Task<TripDto> AssignOrdersToTripAsync(AssignOrdersToTripDto dto)
        {
            var trip = await _tripRepository.GetByIdAsync(dto.TripId);
            if (trip == null) throw AppException.NotFound(ErrorCodes.TripNotFound);

            foreach (var orderId in dto.OrderIds)
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order != null)
                {
                    if (order.TripId != null && order.TripId != dto.TripId)
                    {
                         throw new AppException(ErrorCodes.TripOrderAlreadyAssigned, 400, order.ReferenceNumber);
                    }
                    order.Status = OrderStatus.Assigned;
                    if (!trip.Orders.Any(o => o.Id == orderId))
                    {
                        trip.Orders.Add(order);
                    }
                }
            }

            if (!string.IsNullOrEmpty(dto.RouteJson)) trip.RouteJson = dto.RouteJson;
            if (dto.StartLocationLat.HasValue) trip.StartLocationLat = dto.StartLocationLat.Value;
            if (dto.StartLocationLng.HasValue) trip.StartLocationLng = dto.StartLocationLng.Value;

            await _tripRepository.UpdateAsync(trip);
            return MapToDto(trip);
        }

        public async Task<TripDto> UpdateTripStatusAsync(Guid id, UpdateTripStatusDto dto)
        {
            var trip = await _tripRepository.GetByIdAsync(id);
            if (trip == null) throw AppException.NotFound(ErrorCodes.TripNotFound);

            if (Enum.TryParse<TripStatus>(dto.Status, true, out var status))
            {
                trip.Status = status;
                await _tripRepository.UpdateAsync(trip);
            }
            return MapToDto(trip);
        }

        public async Task<TripDto> UpdateTripCleanerAsync(Guid id, UpdateTripCleanerDto dto)
        {
            var trip = await _tripRepository.GetByIdAsync(id);
            if (trip == null) throw AppException.NotFound(ErrorCodes.TripNotFound);
            trip.CleanerId = dto.CleanerId;
            await _tripRepository.UpdateAsync(trip);
            return MapToDto(trip);
        }

        public async Task<TripDto> UpdateTripDriverAsync(Guid id, UpdateTripDriverDto dto)
        {
            var trip = await _tripRepository.GetByIdAsync(id);
            if (trip == null) throw AppException.NotFound(ErrorCodes.TripNotFound);
            trip.AssignedDriverId = dto.DriverId;
            await _tripRepository.UpdateAsync(trip);
            return MapToDto(trip);
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

        public async Task<List<TripDto>> GetTripsForDriverAsync(Guid driverId)
        {
            var trips = await _tripRepository.GetByDriverIdAsync(driverId);
            return trips.Select(MapToDto).ToList();
        }

        public async Task<TripDto?> GetTripByOrderIdAsync(Guid orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order?.TripId == null) return null;

            var trip = await _tripRepository.GetByIdAsync(order.TripId.Value);
            return trip != null ? MapToDto(trip) : null;
        }

        public async Task DeleteTripAsync(Guid id)
        {
            var trip = await _tripRepository.GetByIdAsync(id);
            if (trip == null)
            {
                throw AppException.NotFound(ErrorCodes.TripNotFound);
            }

            await _tripRepository.DeleteAsync(id);
        }

        private TripDto MapToDto(Trip trip)
        {
            var language = _language.Language;
            return new TripDto
            {
                Id = trip.Id,
                ReferenceNumber = trip.ReferenceNumber,
                Status = trip.Status.ToString(),
                CleanerId = trip.CleanerId,
                CleanerName = trip.Cleaner != null ? BilingualText.Pick(trip.Cleaner.NameAr, trip.Cleaner.NameEn, language) : null,
                DriverId = trip.AssignedDriverId,
                DriverName = trip.Driver != null ? BilingualText.Pick(trip.Driver.NameAr, trip.Driver.NameEn, language) : null,
                CreatedAt = trip.CreatedAt,
                RouteJson = trip.RouteJson,
                StartedAt = trip.StartedAt,
                StartLocationLat = trip.StartLocationLat,
                StartLocationLng = trip.StartLocationLng,
                CleanerLat = trip.Cleaner?.Latitude,
                CleanerLng = trip.Cleaner?.Longitude,
                OrderCount = trip.Orders.Count,
                Orders = trip.Orders.Select(o => OrderService.MapToDto(o, language)).ToList()
            };
        }
    }
}
