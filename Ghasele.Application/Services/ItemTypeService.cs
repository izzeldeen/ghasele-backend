using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;
using Ghasele.Application.Exceptions;
using Ghasele.Application.Interfaces;
using Ghasele.Application.Localization;
using Ghasele.Domain.Entities;
using Ghasele.Domain.Interfaces;

namespace Ghasele.Application.Services
{
    public class ItemTypeService : IItemTypeService
    {
        private readonly IItemTypeRepository _repository;

        public ItemTypeService(IItemTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<ItemTypeDto> CreateItemTypeAsync(CreateItemTypeDto dto)
        {
            var itemType = new ItemType
            {
                TypeName = dto.TypeName,
                IronPrice = dto.IronPrice,
                IronCost = dto.IronCost,
                CleaningPrice = dto.CleaningPrice,
                CleaningCost = dto.CleaningCost,
                BothPrice = dto.BothPrice,
                BothCost = dto.BothCost
            };

            await _repository.AddAsync(itemType);

            return new ItemTypeDto
            {
                Id = itemType.Id,
                TypeName = itemType.TypeName,
                IronPrice = itemType.IronPrice,
                IronCost = itemType.IronCost,
                CleaningPrice = itemType.CleaningPrice,
                CleaningCost = itemType.CleaningCost,
                BothPrice = itemType.BothPrice,
                BothCost = itemType.BothCost
            };
        }

        public async Task<List<ItemTypeDto>> GetAllItemTypesAsync()
        {
            var items = await _repository.GetAllAsync();
            return items.Select(i => new ItemTypeDto
            {
                Id = i.Id,
                TypeName = i.TypeName,
                IronPrice = i.IronPrice,
                IronCost = i.IronCost,
                CleaningPrice = i.CleaningPrice,
                CleaningCost = i.CleaningCost,
                BothPrice = i.BothPrice,
                BothCost = i.BothCost
            }).ToList();
        }

        public async Task<ItemTypeDto> UpdateItemTypeAsync(Guid id, CreateItemTypeDto dto)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null) throw AppException.NotFound(ErrorCodes.ItemTypeNotFound);

            item.TypeName = dto.TypeName;
            item.IronPrice = dto.IronPrice;
            item.IronCost = dto.IronCost;
            item.CleaningPrice = dto.CleaningPrice;
            item.CleaningCost = dto.CleaningCost;
            item.BothPrice = dto.BothPrice;
            item.BothCost = dto.BothCost;

            await _repository.UpdateAsync(item);

            return new ItemTypeDto
            {
                Id = item.Id,
                TypeName = item.TypeName,
                IronPrice = item.IronPrice,
                IronCost = item.IronCost,
                CleaningPrice = item.CleaningPrice,
                CleaningCost = item.CleaningCost,
                BothPrice = item.BothPrice,
                BothCost = item.BothCost
            };
        }

        public async Task DeleteItemTypeAsync(Guid id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item != null)
            {
                await _repository.DeleteAsync(item);
            }
        }
    }
}
