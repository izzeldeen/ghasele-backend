using System;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;
using Ghasele.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ghasele.API.Controllers
{
    [ApiController]
    [Route("api/user-locations")]
    [Authorize]
    public class UserLocationsController : ControllerBase
    {
        private readonly IUserLocationService _userLocationService;

        public UserLocationsController(IUserLocationService userLocationService)
        {
            _userLocationService = userLocationService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserLocations(Guid userId)
        {
            try
            {
                var locations = await _userLocationService.GetUserLocationsAsync(userId);
                return Ok(locations);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddUserLocation([FromBody] CreateUserLocationDto dto)
        {
            try
            {
                var location = await _userLocationService.AddLocationAsync(dto);
                return Ok(location);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserLocation(Guid id)
        {
            try
            {
                await _userLocationService.DeleteLocationAsync(id);
                return Ok(new { message = "Location deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
