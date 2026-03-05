using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Domain.Entities;

namespace Ghasele.Domain.Interfaces
{
    public interface ISupportTicketRepository
    {
        Task<SupportTicket> CreateAsync(SupportTicket ticket);
        Task<IEnumerable<SupportTicket>> GetByUserIdAsync(string userId);
        Task<SupportTicket?> GetByIdAsync(int id);
        Task UpdateAsync(SupportTicket ticket);
        Task<IEnumerable<SupportTicket>> GetAllAsync();
    }
}
