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
    public class DriverRepository : IDriverRepository
    {
        private readonly ApplicationDbContext _context;

        public DriverRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Driver> AddAsync(Driver driver)
        {
            await _context.Drivers.AddAsync(driver);
            await _context.SaveChangesAsync();
            return driver;
        }

        public async Task<List<Driver>> GetAllAsync()
        {
            return await _context.Drivers
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<Driver?> GetByIdAsync(Guid id)
        {
            return await _context.Drivers.FindAsync(id);
        }

        public async Task UpdateAsync(Driver driver)
        {
            _context.Drivers.Update(driver);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var driver = await _context.Drivers.FindAsync(id);
            if (driver != null)
            {
                _context.Drivers.Remove(driver);
                await _context.SaveChangesAsync();
            }
        }
    }
}
