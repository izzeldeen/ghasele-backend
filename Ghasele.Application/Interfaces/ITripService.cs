using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;
using Ghasele.Domain.Entities;

namespace Ghasele.Application.Interfaces
{
    public interface ITripService
    {
        Task<TripDto> CreateTripAsync(CreateTripDto dto);
        Task<TripDto> AssignOrdersToTripAsync(AssignOrdersToTripDto dto);
        Task<TripDto> UpdateTripStatusAsync(Guid id, UpdateTripStatusDto dto);
        Task<TripDto> UpdateTripCleanerAsync(Guid id, UpdateTripCleanerDto dto);
        Task<TripDto> UpdateTripDriverAsync(Guid id, UpdateTripDriverDto dto);
        Task<TripDto> CollectOrderAsync(Guid orderId);
        Task<TripDto> DeliverToCleanerAsync(Guid tripId);
        Task<TripDto> UpdateOrderInTripStatusAsync(Guid orderId, Ghasele.Domain.Entities.OrderStatus status);
        Task<TripDto> DeliverOrderAsync(Guid orderId);
        Task<List<TripDto>> GetAllTripsAsync();
        Task<TripDto?> GetTripByIdAsync(Guid id);
    }
}
