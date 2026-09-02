using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ghasele.Domain.Entities;
using Ghasele.Domain.Interfaces;
using Ghasele.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ghasele.Infrastructure.Repositories
{
    public class DeliveryWindowRepository : IDeliveryWindowRepository
    {
        private readonly ApplicationDbContext _context;

        public DeliveryWindowRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<List<DeliveryWindow>> GetAllAsync() =>
            _context.DeliveryWindows.OrderBy(w => w.StartTime).ToListAsync();

        public Task<List<DeliveryWindow>> GetActiveAsync() =>
            _context.DeliveryWindows.Where(w => w.IsActive).OrderBy(w => w.StartTime).ToListAsync();

        public Task<DeliveryWindow?> GetByIdAsync(Guid id) =>
            _context.DeliveryWindows.FirstOrDefaultAsync(w => w.Id == id);

        public async Task<DeliveryWindow> AddAsync(DeliveryWindow window)
        {
            await _context.DeliveryWindows.AddAsync(window);
            await _context.SaveChangesAsync();
            return window;
        }

        public async Task UpdateAsync(DeliveryWindow window)
        {
            _context.DeliveryWindows.Update(window);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var window = await _context.DeliveryWindows.FindAsync(id);
            if (window != null)
            {
                _context.DeliveryWindows.Remove(window);
                await _context.SaveChangesAsync();
            }
        }
    }
}
