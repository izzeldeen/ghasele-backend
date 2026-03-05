using Ghasele.Domain.Entities;
using Ghasele.Domain.Interfaces;
using Ghasele.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ghasele.Infrastructure.Repositories
{
    public class CleanerRepository : ICleanerRepository
    {
        private readonly ApplicationDbContext _context;

        public CleanerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Cleaner> AddAsync(Cleaner cleaner)
        {
            await _context.Cleaners.AddAsync(cleaner);
            await _context.SaveChangesAsync();
            return cleaner;
        }

        public async Task<List<Cleaner>> GetAllAsync()
        {
            return await _context.Cleaners
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Cleaner?> GetByIdAsync(Guid id)
        {
            return await _context.Cleaners.FindAsync(id);
        }

        public async Task UpdateAsync(Cleaner cleaner)
        {
            _context.Cleaners.Update(cleaner);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var cleaner = await _context.Cleaners.FindAsync(id);
            if (cleaner != null)
            {
                _context.Cleaners.Remove(cleaner);
                await _context.SaveChangesAsync();
            }
        }
    }
}
