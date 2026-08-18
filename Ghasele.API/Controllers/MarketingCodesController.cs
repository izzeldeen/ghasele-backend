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
    [Authorize] // Should probably be restricted to Admins later
    public class MarketingCodesController : ApiControllerBase
    {
        private readonly IMarketingCodeService _marketingCodeService;

        public MarketingCodesController(IMarketingCodeService marketingCodeService)
        {
            _marketingCodeService = marketingCodeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var codes = await _marketingCodeService.GetAllAsync();
            return Ok(codes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var code = await _marketingCodeService.GetByIdAsync(id);
            if (code == null) return NotFound();
            return Ok(code);
        }

        [HttpGet("code/{code}")]
        public async Task<IActionResult> GetByCode(string code)
        {
            var marketingCode = await _marketingCodeService.GetByCodeAsync(code);
            if (marketingCode == null) return NotFound(new { errorCode = ErrorCodes.MarketingCodeInvalidOrInactive, message = L(ErrorCodes.MarketingCodeInvalidOrInactive) });
            return Ok(marketingCode);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMarketingCodeDto dto)
        {
            try
            {
                var code = await _marketingCodeService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = code.Id }, code);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMarketingCodeDto dto)
        {
            try
            {
                var code = await _marketingCodeService.UpdateAsync(id, dto);
                return Ok(code);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _marketingCodeService.DeleteAsync(id);
            return Ok(new { message = L(ErrorCodes.MarketingCodeDeleted) });
        }
    }
}
