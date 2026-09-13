using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Domain.Entities;
using Ghasele.Domain.Interfaces;
using Ghasele.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ghasele.Infrastructure.Repositories
{
    public class ItemTypeRepository : IItemTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public ItemTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ItemType> AddAsync(ItemType itemType)
        {
            await _context.ItemTypes.AddAsync(itemType);
            await _context.SaveChangesAsync();
            return itemType;
        }

        /// <summary>
        /// The catalogue in the order the customer should read it.
        /// </summary>
        /// <remarks>
        /// Ordered here rather than at each call site because every client - the pricing
        /// page in the customer app, the captain's item picker, the dashboard - renders
        /// this list exactly as it arrives. Without an ORDER BY the database is free to
        /// return the rows however it likes, so the pricing page could reshuffle itself
        /// between two visits. Name breaks the tie so rows nobody has arranged yet (all
        /// still at 0) read alphabetically instead of arbitrarily.
        /// </remarks>
        public async Task<List<ItemType>> GetAllAsync()
        {
            return await _context.ItemTypes
                .Where(i => !i.IsDeleted)
                .OrderBy(i => i.SortOrder)
                .ThenBy(i => i.TypeNameEn)
                .ToListAsync();
        }

        public async Task<ItemType?> GetByIdAsync(Guid id)
        {
            return await _context.ItemTypes.FindAsync(id);
        }

        public async Task UpdateAsync(ItemType itemType)
        {
            _context.ItemTypes.Update(itemType);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ItemType itemType)
        {
            itemType.IsDeleted = true;
            _context.ItemTypes.Update(itemType);
            await _context.SaveChangesAsync();
        }
    }
}
