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
    public class CleanerItemPriceRepository : ICleanerItemPriceRepository
    {
        private readonly ApplicationDbContext _context;

        public CleanerItemPriceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CleanerItemPrice>> GetByCleanerAsync(Guid cleanerId)
        {
            return await _context.CleanerItemPrices
                .Include(p => p.ItemType)
                .Where(p => p.CleanerId == cleanerId)
                .ToListAsync();
        }

        public async Task SaveForCleanerAsync(Guid cleanerId, IEnumerable<CleanerItemPrice> prices)
        {
            var incoming = prices.ToList();
            var existing = await _context.CleanerItemPrices
                .Where(p => p.CleanerId == cleanerId)
                .ToListAsync();

            foreach (var price in incoming)
            {
                var current = existing.FirstOrDefault(p => p.ItemTypeId == price.ItemTypeId);
                if (current == null)
                {
                    _context.CleanerItemPrices.Add(new CleanerItemPrice
                    {
                        CleanerId = cleanerId,
                        ItemTypeId = price.ItemTypeId,
                        IronPrice = price.IronPrice,
                        CleaningPrice = price.CleaningPrice,
                        BothPrice = price.BothPrice,
                        CreatedAt = DateTime.UtcNow
                    });
                    continue;
                }

                // Updated in place so the row keeps its identity - nothing points at these by id
                // today, but replacing rows on every save would churn the table for no reason.
                current.IronPrice = price.IronPrice;
                current.CleaningPrice = price.CleaningPrice;
                current.BothPrice = price.BothPrice;
                current.UpdatedAt = DateTime.UtcNow;
            }

            var incomingItemTypeIds = incoming.Select(p => p.ItemTypeId).ToHashSet();
            var removed = existing.Where(p => !incomingItemTypeIds.Contains(p.ItemTypeId)).ToList();
            if (removed.Count > 0)
            {
                _context.CleanerItemPrices.RemoveRange(removed);
            }

            await _context.SaveChangesAsync();
        }
    }
}
