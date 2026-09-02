using System;
using System.Threading.Tasks;
using Ghasele.Domain.Entities;
using Ghasele.Domain.Interfaces;
using Ghasele.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ghasele.Infrastructure.Repositories
{
    public class AppSettingsRepository : IAppSettingsRepository
    {
        private readonly ApplicationDbContext _context;

        public AppSettingsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AppSettings> GetAsync()
        {
            var settings = await _context.AppSettings.FirstOrDefaultAsync();
            if (settings != null) return settings;

            // The row is seeded by migration, so this only fires against a database
            // restored without seed data. Recreate it rather than letting every caller
            // downstream deal with a null and fall back to its own hardcoded price.
            settings = new AppSettings
            {
                Id = AppSettings.SingletonId,
                NormalDeliveryPrice = 1.00m,
                ExpressDeliveryPrice = 1.00m,
                UpdatedAt = DateTime.UtcNow
            };
            await _context.AppSettings.AddAsync(settings);
            await _context.SaveChangesAsync();
            return settings;
        }

        public async Task UpdateAsync(AppSettings settings)
        {
            settings.UpdatedAt = DateTime.UtcNow;
            _context.AppSettings.Update(settings);
            await _context.SaveChangesAsync();
        }
    }
}
