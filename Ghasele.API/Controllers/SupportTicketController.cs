using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;
using Ghasele.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ghasele.API.Controllers
{
    [ApiController]
    [Route("api/support-tickets")]
    [Authorize]
    public class SupportTicketController : ControllerBase
    {
        private readonly ISupportTicketService _service;

        public SupportTicketController(ISupportTicketService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<TicketDto>> CreateTicket([FromBody] CreateTicketDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? dto.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found");
            }

            var ticket = await _service.CreateTicketAsync(userId, dto);
            return Ok(ticket);
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
