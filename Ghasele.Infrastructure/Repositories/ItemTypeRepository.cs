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

        public async Task<List<ItemType>> GetAllAsync()
        {
            return await _context.ItemTypes
                .Where(i => !i.IsDeleted)
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
