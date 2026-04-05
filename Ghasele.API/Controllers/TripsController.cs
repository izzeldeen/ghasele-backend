using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;
using Ghasele.Application.Interfaces;
using Ghasele.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ghasele.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TripsController : ControllerBase
    {
        private readonly ITripService _tripService;

        public TripsController(ITripService tripService)
        {
            _tripService = tripService;
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
                return BadRequest(new { message = ex.Message });
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
                return BadRequest(new { message = ex.Message });
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
                return BadRequest(new { message = ex.Message });
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
                return BadRequest(new { message = ex.Message });
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
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("orders/{orderId}/collect")]
        public async Task<IActionResult> CollectOrder(Guid orderId)
        {
            try
            {
                var trip = await _tripService.CollectOrderAsync(orderId);
                return Ok(trip);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/deliver-to-cleaner")]
        public async Task<IActionResult> DeliverToCleaner(Guid id)
        {
            try
            {
                var trip = await _tripService.DeliverToCleanerAsync(id);
                return Ok(trip);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("orders/{orderId}/status")]
        public async Task<IActionResult> UpdateOrderInTripStatus(Guid orderId, [FromBody] OrderStatus status)
        {
            try
            {
                var trip = await _tripService.UpdateOrderInTripStatusAsync(orderId, status);
                return Ok(trip);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("orders/{orderId}/deliver")]
        public async Task<IActionResult> DeliverOrder(Guid orderId)
        {
            try
            {
                var trip = await _tripService.DeliverOrderAsync(orderId);
                return Ok(trip);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
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
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTripById(Guid id)
        {
            try
            {
                var trip = await _tripService.GetTripByIdAsync(id);
                if (trip == null) return NotFound(new { message = "Trip not found" });
                return Ok(trip);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
