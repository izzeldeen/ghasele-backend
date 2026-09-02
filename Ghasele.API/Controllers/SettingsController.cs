using System;
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
    public class SettingsController : ApiControllerBase
    {
        private readonly IAppSettingsService _service;

        public SettingsController(IAppSettingsService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                return Ok(await _service.GetAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateAppSettingsDto dto)
        {
            try
            {
                return Ok(await _service.UpdateAsync(dto));
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        /// <summary>
        /// Prices only, so the customer app can label the Normal/Express choice.
        /// Any signed-in customer may read this; the full settings object above is
        /// admin-facing.
        /// </summary>
        [HttpGet("delivery-pricing")]
        public async Task<IActionResult> GetDeliveryPricing()
        {
            try
            {
                return Ok(await _service.GetDeliveryPricingAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }
    }
}
