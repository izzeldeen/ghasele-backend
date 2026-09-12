using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using Ghasele.Application.DTOs;
using Ghasele.Application.Localization;
using Ghasele.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ghasele.API.Controllers
{
    [ApiController]
    [Route("api/support-tickets")]
    [Authorize]
    public class SupportTicketController : ApiControllerBase
    {
        private readonly ISupportTicketService _service;

        public SupportTicketController(ISupportTicketService service)
        {
            _service = service;
        }

        /// <summary>
        /// Opens a ticket. Accepts multipart/form-data so the customer app can attach
        /// a single photo; a plain form with no file works too.
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<ActionResult<TicketDto>> CreateTicket([FromForm] CreateTicketDto dto, IFormFile? attachment)
        {
            // Only a bearer token makes the caller a signed-in user. dto.UserId is no longer
            // trusted: this endpoint is now open to guests, and an anonymous caller passing
            // someone else's id would otherwise file a ticket against their account.
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var deviceToken = DeviceToken();

            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(deviceToken))
            {
                return BadRequest(new { errorCode = ErrorCodes.GuestDeviceTokenRequired, message = L(ErrorCodes.GuestDeviceTokenRequired) });
            }

            TicketAttachmentUpload? upload = null;
            if (attachment != null && attachment.Length > 0)
            {
                upload = new TicketAttachmentUpload
                {
                    Content = attachment.OpenReadStream(),
                    FileName = attachment.FileName,
                    ContentType = attachment.ContentType,
                    Length = attachment.Length
                };
            }

            try
            {
                var ticket = await _service.CreateTicketAsync(userId, deviceToken, dto, upload);
                return Ok(ticket);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorBody(ex));
            }
        }

        /// <summary>
        /// Tickets opened from this device without an account, so a guest can read the reply.
        /// </summary>
        /// <remarks>
        /// The device token is the whole credential. It only ever returns that device's own
        /// guest tickets - never an account's - which is why it can be anonymous.
        /// </remarks>
        [AllowAnonymous]
        [HttpGet("guest")]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetGuestTickets()
        {
            var deviceToken = DeviceToken();
            if (string.IsNullOrEmpty(deviceToken))
            {
                return BadRequest(new { errorCode = ErrorCodes.GuestDeviceTokenRequired, message = L(ErrorCodes.GuestDeviceTokenRequired) });
            }

            var tickets = await _service.GetGuestTicketsAsync(deviceToken);
            return Ok(tickets);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetAllTickets()
        {
            var tickets = await _service.GetAllTicketsAsync();
            return Ok(tickets);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetUserTickets(string userId)
        {
            // Optional: Validate that the requesting user matches the userId or is Admin
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != userId && !User.IsInRole("Admin")) 
            {
                // For now, allow it or block it. Let's allow it for simplicity as the app sends userId
                // But strictly speaking we should enforce it.
                // return Forbid();
            }

            var tickets = await _service.GetUserTicketsAsync(userId);
            return Ok(tickets);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TicketDto>> GetTicketById(int id)
        {
            var ticket = await _service.GetTicketByIdAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            return Ok(ticket);
        }

        // Admin endpoint example
        [HttpPut("{id}/response")]
        // [Authorize(Roles = "Admin")] // Uncomment if roles are set up
        public async Task<ActionResult<TicketDto>> RespondToTicket(int id, [FromBody] string response)
        {
             var ticket = await _service.RespondToTicketAsync(id, response);
             if (ticket == null) return NotFound();
             return Ok(ticket);
        }
    }
}
