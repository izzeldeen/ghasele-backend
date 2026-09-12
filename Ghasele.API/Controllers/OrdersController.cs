using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
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

        /// <summary>
        /// Places an order. Open to guests, unlike the rest of this controller.
        /// </summary>
        /// <remarks>
        /// The owner is always taken from the bearer token, never from the body: an anonymous
        /// caller could otherwise pass any customer's id and file an order against their account.
        /// No token means a guest order, which the service requires a contact number for.
        /// </remarks>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            try
            {
                // Same claim pair AuthController.DeleteAccount reads, so both agree on who the
                // caller is regardless of which one issued the token.
                var callerId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                               ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                dto.UserId = Guid.TryParse(callerId, out var callerGuid) ? callerGuid : null;

                // Header, not body, for the same reason as the owner: it is the guest's only
                // handle on their orders, and taking it from one place keeps create and list
                // reading the same value.
                dto.DeviceToken = DeviceToken();

                var order = await _orderService.CreateOrderAsync(dto);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        /// <summary>
        /// Orders placed from this device without an account, so a guest can still track them.
        /// </summary>
        /// <remarks>
        /// The device token is the whole credential here. It is a random per-install value that
        /// never leaves the device except in this header, and it can only ever return that
        /// device's own guest orders - never an account's.
        /// </remarks>
        [AllowAnonymous]
        [HttpGet("guest")]
        public async Task<IActionResult> GetGuestOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var deviceToken = DeviceToken();
                if (string.IsNullOrWhiteSpace(deviceToken))
                {
                    return BadRequest(new { errorCode = ErrorCodes.GuestDeviceTokenRequired, message = L(ErrorCodes.GuestDeviceTokenRequired) });
                }

                var orders = await _orderService.GetGuestOrdersAsync(deviceToken, page, pageSize);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        /// <summary>
        /// Re-points this device's open guest orders at the app's current FCM token.
        /// </summary>
        /// <remarks>
        /// The token stamped on an order at checkout is the only way to reach a guest, and FCM
        /// replaces these tokens on reinstall, restore and its own schedule - so the app calls
        /// this on launch and on every token refresh while signed out, and the pushes keep
        /// arriving. Anonymous, and scoped to the caller's own device token exactly like the
        /// guest listing above: it can only ever rewrite rows that device placed.
        /// </remarks>
        [AllowAnonymous]
        [HttpPut("guest/fcm-token")]
        public async Task<IActionResult> UpdateGuestFcmToken([FromBody] UpdateGuestFcmTokenDto dto)
        {
            try
            {
                var deviceToken = DeviceToken();
                if (string.IsNullOrWhiteSpace(deviceToken))
                {
                    return BadRequest(new { errorCode = ErrorCodes.GuestDeviceTokenRequired, message = L(ErrorCodes.GuestDeviceTokenRequired) });
                }

                if (string.IsNullOrWhiteSpace(dto?.FcmToken))
                {
                    return BadRequest(new { errorCode = ErrorCodes.GuestFcmTokenRequired, message = L(ErrorCodes.GuestFcmTokenRequired) });
                }

                var updated = await _orderService.UpdateGuestFcmTokenAsync(deviceToken, dto.FcmToken);
                return Ok(new { updated });
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        /// <summary>Per-install identifier the app sends on every request, or null if absent.</summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserOrders(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var orders = await _orderService.GetUserOrdersAsync(userId, page, pageSize);
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

        [HttpDelete("{id}/items/{itemId}")]
        public async Task<IActionResult> DeleteOrderItem(Guid id, Guid itemId)
        {
            try
            {
                var order = await _orderService.DeleteOrderItemAsync(id, itemId);
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
