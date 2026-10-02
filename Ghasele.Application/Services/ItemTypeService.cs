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

        /// <summary>
        /// Refuses a ceiling that sits below the base price it belongs to.
        /// </summary>
        /// <remarks>
        /// Checked here rather than left to the display rule, because there is no sensible
        /// way to show "from 10 to 3": every client would have to invent its own answer, and
        /// they would not agree. A ceiling equal to the base is allowed and means a fixed
        /// price; null means the price is open-ended.
        /// </remarks>
        private static void EnsurePriceRangeValid(CreateItemTypeDto dto)
        {
            if (dto.MaxPrice.HasValue && dto.MaxPrice.Value < dto.Price)
            {
                throw new AppException(ErrorCodes.ItemTypePriceRangeInvalid);
            }
        }

        public async Task<ItemTypeDto> CreateItemTypeAsync(CreateItemTypeDto dto)
        {
            EnsurePriceRangeValid(dto);

            var itemType = new ItemType
            {
                TypeNameAr = dto.TypeNameAr,
                TypeNameEn = dto.TypeNameEn,
                Price = dto.Price,
                MaxPrice = dto.MaxPrice,
                SortOrder = dto.SortOrder ?? await NextSortOrderAsync(),
            };

            await _repository.AddAsync(itemType);

            return MapToDto(itemType);
        }

        /// <summary>
        /// One past the last item in the catalogue, so an item added without a position
        /// lands at the end.
        /// </summary>
        /// <remarks>
        /// Defaulting to 0 instead would drop every new item straight to the top of the
        /// customer's pricing page, which is the opposite of what adding one usually means.
        /// </remarks>
        private async Task<int> NextSortOrderAsync()
        {
            var existing = await _repository.GetAllAsync();
            return existing.Count == 0 ? 0 : existing.Max(i => i.SortOrder) + 1;
        }

        public async Task<List<ItemTypeDto>> GetAllItemTypesAsync()
        {
            var items = await _repository.GetAllAsync();
            return items.Select(MapToDto).ToList();
        }

        public async Task<ItemTypeDto> UpdateItemTypeAsync(Guid id, CreateItemTypeDto dto)
        {
            EnsurePriceRangeValid(dto);

            var item = await _repository.GetByIdAsync(id);
            if (item == null) throw AppException.NotFound(ErrorCodes.ItemTypeNotFound);

            item.TypeNameAr = dto.TypeNameAr;
            item.TypeNameEn = dto.TypeNameEn;
            item.Price = dto.Price;
            item.MaxPrice = dto.MaxPrice;
            // Left where it is when the caller says nothing, so an edit that only touches
            // prices cannot quietly move the item on the customer's pricing page.
            if (dto.SortOrder.HasValue) item.SortOrder = dto.SortOrder.Value;

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
                Price = item.Price,
                MaxPrice = item.MaxPrice,
                SortOrder = item.SortOrder,
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
