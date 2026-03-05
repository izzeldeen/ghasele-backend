using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;

namespace Ghasele.Application.Interfaces
{
    public interface IItemTypeService
    {
        Task<ItemTypeDto> CreateItemTypeAsync(CreateItemTypeDto dto);
        Task<ItemTypeDto> UpdateItemTypeAsync(Guid id, CreateItemTypeDto dto);
        Task<List<ItemTypeDto>> GetAllItemTypesAsync();
        Task DeleteItemTypeAsync(Guid id);
    }
}
