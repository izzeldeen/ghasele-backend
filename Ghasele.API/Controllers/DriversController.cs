using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;
using Ghasele.Application.Interfaces;
using Ghasele.Application.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ghasele.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DriversController : ApiControllerBase
    {
        private readonly IDriverService _driverService;

        public DriversController(IDriverService driverService)
        {
            _driverService = driverService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDriver([FromBody] CreateDriverDto dto)
        {
            try
            {
                var driver = await _driverService.CreateDriverAsync(dto);
                return Ok(driver);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDrivers()
        {
            try
            {
                var drivers = await _driverService.GetAllDriversAsync();
                return Ok(drivers);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDriverById(Guid id)
        {
            try
            {
                var driver = await _driverService.GetDriverByIdAsync(id);
                if (driver == null) return NotFound(new { errorCode = ErrorCodes.DriverNotFound, message = L(ErrorCodes.DriverNotFound) });
                return Ok(driver);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDriver(Guid id, [FromBody] UpdateDriverDto dto)
        {
            try
            {
                var driver = await _driverService.UpdateDriverAsync(id, dto);
                return Ok(driver);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDriver(Guid id)
        {
            try
            {
                await _driverService.DeleteDriverAsync(id);
                return Ok(new { message = L(ErrorCodes.DriverDeleted) });
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }
    }
}
