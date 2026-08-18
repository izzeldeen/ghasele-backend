using System;
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
    // [Authorize] // Uncomment if you want to protect this route
    public class ItemTypesController : ApiControllerBase
    {
        private readonly IItemTypeService _service;

        public ItemTypesController(IItemTypeService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateItemTypeDto dto)
        {
            try
            {
                var result = await _service.CreateItemTypeAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAllItemTypesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateItemTypeDto dto)
        {
            try
            {
                var result = await _service.UpdateItemTypeAsync(id, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
             try
            {
                await _service.DeleteItemTypeAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }
    }
}
