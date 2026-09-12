using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;
using Ghasele.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ghasele.API.Controllers
{
    /// <summary>
    /// Recurring daily delivery windows ("scheduled trips"). Managed from the dashboard;
    /// the customer app reads the projected slots via <see cref="GetSlots"/>.
    /// </summary>
    [ApiController]
    [Route("api/delivery-windows")]
    [Authorize]
    public class DeliveryWindowsController : ApiControllerBase
    {
        private readonly IDeliveryWindowService _service;

        public DeliveryWindowsController(IDeliveryWindowService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try { return Ok(await _service.GetAllAsync()); }
            catch (Exception ex) { return BadRequest(ErrorBody(ex)); }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDeliveryWindowDto dto)
        {
            try { return Ok(await _service.CreateAsync(dto)); }
            catch (Exception ex) { return BadRequest(ErrorBody(ex)); }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDeliveryWindowDto dto)
        {
            try { return Ok(await _service.UpdateAsync(id, dto)); }
            catch (Exception ex) { return BadRequest(ErrorBody(ex)); }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex) { return BadRequest(ErrorBody(ex)); }
        }

        /// <summary>Active windows projected across the next N days (default 7, max 30).</summary>
        /// <remarks>
        /// Anonymous: a guest places orders too, and has to see the schedule before there is
        /// any account to authenticate. It exposes only the operator's published opening
        /// times and how full they are - nothing about any customer.
        /// </remarks>
        [AllowAnonymous]
        [HttpGet("slots")]
        public async Task<IActionResult> GetSlots([FromQuery] int days = 7)
        {
            try { return Ok(await _service.GetUpcomingSlotsAsync(days)); }
            catch (Exception ex) { return BadRequest(ErrorBody(ex)); }
        }
    }
}
