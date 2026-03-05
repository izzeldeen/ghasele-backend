using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ghasele.Domain.Entities;
using Ghasele.Domain.Interfaces;
using Ghasele.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ghasele.Infrastructure.Repositories
{
    public class SupportTicketRepository : ISupportTicketRepository
    {
        private readonly ApplicationDbContext _context;

        public SupportTicketRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SupportTicket> CreateAsync(SupportTicket ticket)
        {
            await _context.SupportTickets.AddAsync(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<IEnumerable<SupportTicket>> GetByUserIdAsync(string userId)
        {
            return await _context.SupportTickets
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<SupportTicket?> GetByIdAsync(int id)
        {
            return await _context.SupportTickets.FindAsync(id);
        }

        public async Task UpdateAsync(SupportTicket ticket)
        {
            _context.SupportTickets.Update(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<SupportTicket>> GetAllAsync()
        {
            return await _context.SupportTickets
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
    }
}
