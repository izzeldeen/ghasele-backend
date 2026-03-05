using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;
using Ghasele.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ghasele.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CleanersController : ControllerBase
    {
        private readonly ICleanerService _cleanerService;

        public CleanersController(ICleanerService cleanerService)
        {
            _cleanerService = cleanerService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCleaner([FromBody] CreateCleanerDto dto)
        {
            try
            {
                var cleaner = await _cleanerService.CreateCleanerAsync(dto);
                return Ok(cleaner);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCleaners()
        {
            try
            {
                var cleaners = await _cleanerService.GetAllCleanersAsync();
                return Ok(cleaners);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCleanerById(Guid id)
        {
            try
            {
                var cleaner = await _cleanerService.GetCleanerByIdAsync(id);
                if (cleaner == null) return NotFound(new { message = "Cleaner not found" });
                return Ok(cleaner);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCleaner(Guid id, [FromBody] UpdateCleanerDto dto)
        {
            try
            {
                var cleaner = await _cleanerService.UpdateCleanerAsync(id, dto);
                return Ok(cleaner);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCleaner(Guid id)
        {
            try
            {
                await _cleanerService.DeleteCleanerAsync(id);
                return Ok(new { message = "Cleaner deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
