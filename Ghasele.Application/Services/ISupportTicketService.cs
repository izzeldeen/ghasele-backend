using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;

namespace Ghasele.Application.Services
{
    public interface ISupportTicketService
    {
        /// <summary>
        /// Opens a ticket for a signed-in user (<paramref name="userId"/>) or, when that is null,
        /// for a guest identified only by <paramref name="deviceToken"/>.
        /// </summary>
        Task<TicketDto> CreateTicketAsync(string? userId, string? deviceToken, CreateTicketDto dto, TicketAttachmentUpload? attachment = null);
        Task<IEnumerable<TicketDto>> GetUserTicketsAsync(string userId);

        /// <summary>Tickets opened from one device without an account, newest first.</summary>
        Task<IEnumerable<TicketDto>> GetGuestTicketsAsync(string deviceToken);
        Task<TicketDto?> GetTicketByIdAsync(int id);
        Task<TicketDto?> RespondToTicketAsync(int id, string response);
        Task<IEnumerable<TicketDto>> GetAllTicketsAsync();
    }
}
