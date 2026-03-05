using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;

namespace Ghasele.Application.Interfaces
{
    public interface ITripService
    {
        Task<TripDto> CreateTripAsync(CreateTripDto dto);
        Task<TripDto> AssignOrdersToTripAsync(AssignOrdersToTripDto dto);
        Task<TripDto> UpdateTripStatusAsync(Guid id, UpdateTripStatusDto dto);
        Task<TripDto> UpdateTripCleanerAsync(Guid id, UpdateTripCleanerDto dto);
        Task<TripDto> UpdateTripDriverAsync(Guid id, UpdateTripDriverDto dto);
        Task<List<TripDto>> GetAllTripsAsync();
        Task<TripDto?> GetTripByIdAsync(Guid id);
    }
}
