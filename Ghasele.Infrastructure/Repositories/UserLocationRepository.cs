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
    public class UserLocationRepository : IUserLocationRepository
    {
        private readonly ApplicationDbContext _context;

        public UserLocationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserLocation> AddAsync(UserLocation location)
        {
            await _context.UserLocations.AddAsync(location);
            await _context.SaveChangesAsync();
            return location;
        }

        public async Task<List<UserLocation>> GetByUserIdAsync(Guid userId)
        {
            return await _context.UserLocations
                .Where(l => l.UserId == userId)
                .ToListAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var location = await _context.UserLocations.FindAsync(id);
            if (location != null)
            {
                _context.UserLocations.Remove(location);
                await _context.SaveChangesAsync();
            }
        }
    }
}
