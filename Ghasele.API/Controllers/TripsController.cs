using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;
using Ghasele.Application.Exceptions;
using Ghasele.Application.Interfaces;
using Ghasele.Application.Localization;
using Ghasele.Domain.Entities;
using Ghasele.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ghasele.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TripsController : ApiControllerBase
    {
        private readonly ITripService _tripService;
        private readonly IDriverRepository _driverRepository;

        public TripsController(ITripService tripService, IDriverRepository driverRepository)
        {
            _tripService = tripService;
            _driverRepository = driverRepository;
        }

        /// <summary>
        /// Trips assigned to the calling driver. The mobile app buckets these into its two
        /// tabs client-side by status, the same way the admin dashboard already splits
        /// collection vs. delivery trips - there is no separate "trip type" field.
        /// </summary>
        [HttpGet("my")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> GetMyTrips()
        {
            try
            {
                var driverId = await ResolveCallerDriverIdAsync();
                if (driverId == null)
                {
                    return BadRequest(new { errorCode = ErrorCodes.DriverNotFound, message = L(ErrorCodes.DriverNotFound) });
                }

                var trips = await _tripService.GetTripsForDriverAsync(driverId.Value);
                return Ok(trips);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        /// <summary>
        /// Captain taps "Start trip": stamp StartedAt, capture the start position, and compute the
        /// optimized route if the trip has none. Body: { "lat": .., "lng": .. } (both optional).
        /// </summary>
        [HttpPost("{id}/start")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> StartTrip(Guid id, [FromBody] StartTripDto body)
        {
            try
            {
                await EnsureCallerOwnsTripAsync(id);
                var trip = await _tripService.StartTripAsync(id, body?.Lat, body?.Lng);
                return Ok(trip);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        public record StartTripDto(double? Lat, double? Lng);

        /// <summary>
        /// Looks up the Driver row linked to the caller's User account. Only meaningful for
        /// callers with the Driver role - the admin dashboard's own calls never hit this.
        /// </summary>
        private async Task<Guid?> ResolveCallerDriverIdAsync()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return null;
            }

            var driver = await _driverRepository.GetByUserIdAsync(userId);
            return driver?.Id;
        }

        /// <summary>
        /// Drivers may only act on trips assigned to them; every other authenticated caller
        /// (the admin dashboard today) is unrestricted, matching existing behaviour.
        /// </summary>
        private async Task EnsureCallerOwnsTripAsync(Guid? tripId)
        {
            if (!User.IsInRole("Driver")) return;

            var driverId = await ResolveCallerDriverIdAsync();
            if (tripId == null || driverId == null)
            {
                throw new AppException(ErrorCodes.TripNotAssignedToCaller, 403);
            }

            var trip = await _tripService.GetTripByIdAsync(tripId.Value);
            if (trip == null || trip.DriverId != driverId)
            {
                throw new AppException(ErrorCodes.TripNotAssignedToCaller, 403);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTrip([FromBody] CreateTripDto dto)
        {
            try
            {
                var trip = await _tripService.CreateTripAsync(dto);
                return Ok(trip);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        [HttpPost("assign-orders")]
        public async Task<IActionResult> AssignOrdersToTrip([FromBody] AssignOrdersToTripDto dto)
        {
            try
            {
                var trip = await _tripService.AssignOrdersToTripAsync(dto);
                return Ok(trip);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateTripStatus(Guid id, [FromBody] UpdateTripStatusDto dto)
        {
            try
            {
                var trip = await _tripService.UpdateTripStatusAsync(id, dto);
                return Ok(trip);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        [HttpPut("{id}/cleaner")]
        public async Task<IActionResult> UpdateTripCleaner(Guid id, [FromBody] UpdateTripCleanerDto dto)
        {
            try
            {
                var trip = await _tripService.UpdateTripCleanerAsync(id, dto);
                return Ok(trip);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        [HttpPut("{id}/driver")]
        public async Task<IActionResult> UpdateTripDriver(Guid id, [FromBody] UpdateTripDriverDto dto)
        {
            try
            {
                var trip = await _tripService.UpdateTripDriverAsync(id, dto);
                return Ok(trip);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        [HttpPost("orders/{orderId}/collect")]
        public async Task<IActionResult> CollectOrder(Guid orderId)
        {
            try
            {
                var owningTrip = await _tripService.GetTripByOrderIdAsync(orderId);
                await EnsureCallerOwnsTripAsync(owningTrip?.Id);

                var trip = await _tripService.CollectOrderAsync(orderId);
                return Ok(trip);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        [HttpPost("{id}/deliver-to-cleaner")]
        public async Task<IActionResult> DeliverToCleaner(Guid id)
        {
            try
            {
                await EnsureCallerOwnsTripAsync(id);

                var trip = await _tripService.DeliverToCleanerAsync(id);
                return Ok(trip);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        [HttpPut("orders/{orderId}/status")]
        public async Task<IActionResult> UpdateOrderInTripStatus(Guid orderId, [FromBody] OrderStatus status)
        {
            try
            {
                var owningTrip = await _tripService.GetTripByOrderIdAsync(orderId);
                await EnsureCallerOwnsTripAsync(owningTrip?.Id);

                var trip = await _tripService.UpdateOrderInTripStatusAsync(orderId, status);
                return Ok(trip);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        [HttpPost("orders/{orderId}/deliver")]
        public async Task<IActionResult> DeliverOrder(Guid orderId)
        {
            try
            {
                var owningTrip = await _tripService.GetTripByOrderIdAsync(orderId);
                await EnsureCallerOwnsTripAsync(owningTrip?.Id);

                var trip = await _tripService.DeliverOrderAsync(orderId);
                return Ok(trip);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTrips()
        {
            try
            {
                var trips = await _tripService.GetAllTripsAsync();
                return Ok(trips);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTripById(Guid id)
        {
            try
            {
                var trip = await _tripService.GetTripByIdAsync(id);
                if (trip == null) return NotFound(new { errorCode = ErrorCodes.TripNotFound, message = L(ErrorCodes.TripNotFound) });
                return Ok(trip);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrip(Guid id)
        {
            try
            {
                await _tripService.DeleteTripAsync(id);
                return Ok(new { message = L(ErrorCodes.TripDeleted) });
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }
    }
}
