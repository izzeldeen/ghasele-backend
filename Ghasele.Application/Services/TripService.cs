using System;
using System.Collections.Generic;
using System.Linq;
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

        public TripService(ITripRepository tripRepository, IOrderRepository orderRepository, INotificationService notificationService, IUserNotificationService userNotificationService)
        {
            _tripRepository = tripRepository;
            _orderRepository = orderRepository;
            _notificationService = notificationService;
            _userNotificationService = userNotificationService;
        }

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
                if (order.User != null)
                {
                    string title = "تحديث طلب";
                    string body = "طلبك الآن في طريقه إليك!";
                    await _userNotificationService.CreateNotificationAsync(order.UserId, title, body);
                    if (!string.IsNullOrEmpty(order.User.FcmToken))
                    {
                        await _notificationService.SendNotificationAsync(order.User.FcmToken, title, body);
                    }
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
                CleanerLat = trip.Cleaner?.Latitude,
                CleanerLng = trip.Cleaner?.Longitude,
                OrderCount = trip.Orders.Count,
                Orders = trip.Orders.Select(OrderService.MapToDto).ToList()
            };
        }
    }
}
