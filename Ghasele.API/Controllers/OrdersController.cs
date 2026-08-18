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
    [Authorize] // Require authentication for all order endpoints
    public class OrdersController : ApiControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            try
            {
                var order = await _orderService.CreateOrderAsync(dto);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserOrders(Guid userId)
        {
            try
            {
                var orders = await _orderService.GetUserOrdersAsync(userId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAllOrders([FromQuery] string? status, [FromQuery] string? searchTerm)
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync(status, searchTerm);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null) return NotFound(new { errorCode = ErrorCodes.OrderNotFound, message = L(ErrorCodes.OrderNotFound) });
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }
        [HttpPost("{id}/items")]
        public async Task<IActionResult> AddItems(Guid id, [FromBody] AddOrderItemsDto dto)
        {
            try
            {
                var order = await _orderService.AddItemsToOrderAsync(id, dto);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] UpdateOrderDto dto)
        {
            try
            {
                var order = await _orderService.UpdateOrderAsync(id, dto);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            try
            {
                await _orderService.DeleteOrderAsync(id);
                return Ok(new { message = L(ErrorCodes.OrderDeleted) });
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }
    }
}
