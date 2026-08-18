using System;
using System.Threading.Tasks;
using Ghasele.Application.Localization;
using Ghasele.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ghasele.API.Controllers
{
    // ==========================================================================
    //  DANGER - DESTRUCTIVE, UNAUTHENTICATED ENDPOINT
    // ==========================================================================
    //  This controller permanently deletes ALL orders, order items and trips.
    //  It is [AllowAnonymous]: no login, no JWT, no role check. Anyone who
    //  learns the URL and token can wipe the data over the public internet.
    //
    //  Two guards keep it from being a live "delete everything" button:
    //    1. Maintenance:PurgeEnabled must be true  (defaults to FALSE)
    //    2. Maintenance:PurgeToken must match the supplied token
    //
    //  Set BOTH in appsettings, run the purge, then set PurgeEnabled back to
    //  false - or delete this file entirely. Do not leave it enabled.
    // ==========================================================================
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class MaintenanceController : ApiControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<MaintenanceController> _logger;

        public MaintenanceController(
            ApplicationDbContext db,
            IConfiguration config,
            ILogger<MaintenanceController> logger)
        {
            _db = db;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Deletes every OrderItem, Order and Trip. Irreversible.
        /// POST /api/maintenance/purge-orders?token=YOUR_TOKEN
        /// </summary>
        [HttpPost("purge-orders")]
        public async Task<IActionResult> PurgeOrders([FromQuery] string? token)
        {
            var enabled = _config.GetValue<bool>("Maintenance:PurgeEnabled");
            var expectedToken = _config["Maintenance:PurgeToken"];
            var caller = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            if (!enabled)
            {
                _logger.LogWarning("Purge attempt from {Caller} while disabled.", caller);
                // 404 rather than 403 so the endpoint is not discoverable when off.
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(expectedToken) || token != expectedToken)
            {
                _logger.LogWarning("Purge attempt from {Caller} with a bad token.", caller);
                return Unauthorized(new { errorCode = ErrorCodes.InvalidToken, message = L(ErrorCodes.InvalidToken) });
            }

            _logger.LogWarning("ORDER PURGE STARTED by {Caller}.", caller);

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // OrderItems.OrderId cascades from Orders, but delete explicitly so
                // the reported counts are accurate and the code does not depend on
                // the cascade staying in place.
                var orderItems = await _db.OrderItems.ExecuteDeleteAsync();
                var orders = await _db.Orders.ExecuteDeleteAsync();

                // Orders.TripId is SET NULL, so trips are safe to remove after orders.
                var trips = await _db.Trips.ExecuteDeleteAsync();

                await tx.CommitAsync();

                _logger.LogWarning(
                    "ORDER PURGE COMMITTED by {Caller}: {Items} items, {Orders} orders, {Trips} trips.",
                    caller, orderItems, orders, trips);

                return Ok(new
                {
                    message = L(ErrorCodes.PurgeComplete),
                    deleted = new
                    {
                        orderItems,
                        orders,
                        trips
                    }
                });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "ORDER PURGE FAILED - rolled back.");
                // The raw detail stays in the payload: this endpoint is token-guarded and exists for
                // an operator who needs to know why the purge failed.
                return StatusCode(500, new { message = L(ErrorCodes.PurgeFailed), error = ex.Message });
            }
        }

        /// <summary>
        /// Read-only preview of what a purge would delete. Same guards apply.
        /// GET /api/maintenance/purge-orders/preview?token=YOUR_TOKEN
        /// </summary>
        [HttpGet("purge-orders/preview")]
        public async Task<IActionResult> PreviewPurge([FromQuery] string? token)
        {
            if (!_config.GetValue<bool>("Maintenance:PurgeEnabled")) return NotFound();

            var expectedToken = _config["Maintenance:PurgeToken"];
            if (string.IsNullOrWhiteSpace(expectedToken) || token != expectedToken)
                return Unauthorized(new { errorCode = ErrorCodes.InvalidToken, message = L(ErrorCodes.InvalidToken) });

            return Ok(new
            {
                orderItems = await _db.OrderItems.CountAsync(),
                orders = await _db.Orders.CountAsync(),
                trips = await _db.Trips.CountAsync()
            });
        }
    }
}
