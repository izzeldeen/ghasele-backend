using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Domain.Entities;

namespace Ghasele.Domain.Interfaces
{
    public interface IItemTypeRepository
    {
        Task<ItemType> AddAsync(ItemType itemType);
        Task<List<ItemType>> GetAllAsync();
        Task<ItemType?> GetByIdAsync(Guid id);
        Task UpdateAsync(ItemType itemType);
        Task DeleteAsync(ItemType itemType);
    }
}
