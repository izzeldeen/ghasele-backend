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
    public class TripRepository : ITripRepository
    {
        private readonly ApplicationDbContext _context;

        public TripRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Trip> AddAsync(Trip trip)
        {
            await _context.Trips.AddAsync(trip);
            await _context.SaveChangesAsync();
            return trip;
        }

        public async Task<Trip?> GetByIdAsync(Guid id)
        {
            return await _context.Trips
                .Include(t => t.Orders)
                    .ThenInclude(o => o.Items)
                .Include(t => t.Orders)
                    .ThenInclude(o => o.User)
                .Include(t => t.Cleaner)
                .Include(t => t.Driver)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<Trip>> GetAllAsync()
        {
            return await _context.Trips
                .Include(t => t.Orders)
                    .ThenInclude(o => o.User)
                .Include(t => t.Cleaner)
                .Include(t => t.Driver)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateAsync(Trip trip)
        {
            _context.Trips.Update(trip);
            await _context.SaveChangesAsync();
        }
    }
}
