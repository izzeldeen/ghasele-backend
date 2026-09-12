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
    public class CleanersController : ApiControllerBase
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
                return BadRequest(ErrorBody(ex));
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
                return BadRequest(ErrorBody(ex));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCleanerById(Guid id)
        {
            try
            {
                var cleaner = await _cleanerService.GetCleanerByIdAsync(id);
                if (cleaner == null) return NotFound(new { errorCode = ErrorCodes.CleanerNotFound, message = L(ErrorCodes.CleanerNotFound) });
                return Ok(cleaner);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
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
                return BadRequest(ErrorBody(ex));
            }
        }

        /// <summary>
        /// This laundry's rate card: every item type with its customer prices and the rate agreed
        /// with this laundry. Step two of configuring a cleaner.
        /// </summary>
        [HttpGet("{id}/item-prices")]
        public async Task<IActionResult> GetCleanerItemPrices(Guid id)
        {
            try
            {
                var prices = await _cleanerService.GetItemPricesAsync(id);
                return Ok(prices);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        /// <summary>
        /// Replaces this laundry's agreed rates. Customer prices are untouched - those live on
        /// the item type and are set on the Item Types screen.
        /// </summary>
        /// <remarks>
        /// Changing a rate only affects orders priced after the change: an order stamps the rate
        /// it was priced with onto its own lines at collection time.
        /// </remarks>
        [HttpPut("{id}/item-prices")]
        public async Task<IActionResult> SaveCleanerItemPrices(Guid id, [FromBody] SaveCleanerItemPricesDto dto)
        {
            try
            {
                var prices = await _cleanerService.SaveItemPricesAsync(id, dto);
                return Ok(prices);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCleaner(Guid id)
        {
            try
            {
                await _cleanerService.DeleteCleanerAsync(id);
                return Ok(new { message = L(ErrorCodes.CleanerDeleted) });
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }
    }
}
