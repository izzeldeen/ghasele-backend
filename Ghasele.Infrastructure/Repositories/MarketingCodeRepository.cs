using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Domain.Entities;
using Ghasele.Domain.Interfaces;
using Ghasele.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ghasele.Infrastructure.Repositories
{
    public class MarketingCodeRepository : IMarketingCodeRepository
    {
        private readonly ApplicationDbContext _context;

        public MarketingCodeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MarketingCode?> GetByIdAsync(Guid id)
        {
            return await _context.MarketingCodes.FindAsync(id);
        }

        public async Task<MarketingCode?> GetByCodeAsync(string code)
        {
            return await _context.MarketingCodes
                .FirstOrDefaultAsync(m => m.Code.ToLower() == code.ToLower() && m.IsActive);
        }

        public async Task<List<MarketingCode>> GetAllAsync()
        {
            return await _context.MarketingCodes.ToListAsync();
        }

        public async Task AddAsync(MarketingCode marketingCode)
        {
            await _context.MarketingCodes.AddAsync(marketingCode);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(MarketingCode marketingCode)
        {
            _context.MarketingCodes.Update(marketingCode);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var code = await GetByIdAsync(id);
            if (code != null)
            {
                _context.MarketingCodes.Remove(code);
                await _context.SaveChangesAsync();
            }
        }
    }
}
