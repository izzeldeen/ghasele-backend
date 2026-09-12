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
        private readonly ICurrentLanguageProvider _language;

        public ItemTypeService(IItemTypeRepository repository, ICurrentLanguageProvider language)
        {
            _repository = repository;
            _language = language;
        }

        public async Task<ItemTypeDto> CreateItemTypeAsync(CreateItemTypeDto dto)
        {
            var itemType = new ItemType
            {
                TypeNameAr = dto.TypeNameAr,
                TypeNameEn = dto.TypeNameEn,
                IronPrice = dto.IronPrice,
                CleaningPrice = dto.CleaningPrice,
                BothPrice = dto.BothPrice,
            };

            await _repository.AddAsync(itemType);

            return MapToDto(itemType);
        }

        public async Task<List<ItemTypeDto>> GetAllItemTypesAsync()
        {
            var items = await _repository.GetAllAsync();
            return items.Select(MapToDto).ToList();
        }

        public async Task<ItemTypeDto> UpdateItemTypeAsync(Guid id, CreateItemTypeDto dto)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null) throw AppException.NotFound(ErrorCodes.ItemTypeNotFound);

            item.TypeNameAr = dto.TypeNameAr;
            item.TypeNameEn = dto.TypeNameEn;
            item.IronPrice = dto.IronPrice;
            item.CleaningPrice = dto.CleaningPrice;
            item.BothPrice = dto.BothPrice;

            await _repository.UpdateAsync(item);

            return MapToDto(item);
        }

        private ItemTypeDto MapToDto(ItemType item)
        {
            return new ItemTypeDto
            {
                Id = item.Id,
                TypeNameAr = item.TypeNameAr,
                TypeNameEn = item.TypeNameEn,
                TypeName = BilingualText.Pick(item.TypeNameAr, item.TypeNameEn, _language.Language),
                IronPrice = item.IronPrice,
                CleaningPrice = item.CleaningPrice,
                BothPrice = item.BothPrice,
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
