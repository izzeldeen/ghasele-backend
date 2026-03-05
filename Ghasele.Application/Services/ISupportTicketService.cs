using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;

namespace Ghasele.Application.Services
{
    public interface ISupportTicketService
    {
        Task<TicketDto> CreateTicketAsync(string userId, CreateTicketDto dto);
        Task<IEnumerable<TicketDto>> GetUserTicketsAsync(string userId);
        Task<TicketDto?> GetTicketByIdAsync(int id);
        Task<TicketDto?> RespondToTicketAsync(int id, string response);
        Task<IEnumerable<TicketDto>> GetAllTicketsAsync();
    }
}
