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
    public class CleanerService : ICleanerService
    {
        private readonly ICleanerRepository _cleanerRepository;
        private readonly ICleanerItemPriceRepository _cleanerItemPriceRepository;
        private readonly IItemTypeRepository _itemTypeRepository;
        private readonly ICurrentLanguageProvider _language;

        public CleanerService(ICleanerRepository cleanerRepository, ICleanerItemPriceRepository cleanerItemPriceRepository, IItemTypeRepository itemTypeRepository, ICurrentLanguageProvider language)
        {
            _cleanerRepository = cleanerRepository;
            _cleanerItemPriceRepository = cleanerItemPriceRepository;
            _itemTypeRepository = itemTypeRepository;
            _language = language;
        }

        public async Task<CleanerDto> CreateCleanerAsync(CreateCleanerDto dto)
        {
            var cleaner = new Cleaner
            {
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                Note = dto.Note,
                CleaningLocation = dto.CleaningLocation,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                CreatedAt = DateTime.UtcNow
            };

            await _cleanerRepository.AddAsync(cleaner);
            return MapToDto(cleaner);
        }

        public async Task<List<CleanerDto>> GetAllCleanersAsync()
        {
            var cleaners = await _cleanerRepository.GetAllAsync();
            return cleaners.Select(MapToDto).ToList();
        }

        public async Task<CleanerDto?> GetCleanerByIdAsync(Guid id)
        {
            var cleaner = await _cleanerRepository.GetByIdAsync(id);
            return cleaner != null ? MapToDto(cleaner) : null;
        }

        public async Task<CleanerDto> UpdateCleanerAsync(Guid id, UpdateCleanerDto dto)
        {
            var cleaner = await _cleanerRepository.GetByIdAsync(id);
            if (cleaner == null) throw AppException.NotFound(ErrorCodes.CleanerNotFound);

            if (!string.IsNullOrEmpty(dto.NameAr)) cleaner.NameAr = dto.NameAr;
            if (!string.IsNullOrEmpty(dto.NameEn)) cleaner.NameEn = dto.NameEn;
            if (dto.Note != null) cleaner.Note = dto.Note;
            if (dto.CleaningLocation != null) cleaner.CleaningLocation = dto.CleaningLocation;
            if (dto.Latitude.HasValue) cleaner.Latitude = dto.Latitude.Value;
            if (dto.Longitude.HasValue) cleaner.Longitude = dto.Longitude.Value;

            await _cleanerRepository.UpdateAsync(cleaner);
            return MapToDto(cleaner);
        }

        public async Task DeleteCleanerAsync(Guid id)
        {
            await _cleanerRepository.DeleteAsync(id);
        }

        public async Task<List<CleanerItemPriceDto>> GetItemPricesAsync(Guid cleanerId)
        {
            var cleaner = await _cleanerRepository.GetByIdAsync(cleanerId);
            if (cleaner == null) throw AppException.NotFound(ErrorCodes.CleanerNotFound);

            var itemTypes = await _itemTypeRepository.GetAllAsync();
            var agreed = await _cleanerItemPriceRepository.GetByCleanerAsync(cleanerId);

            // Driven by the item type list, not by the agreed rows: the screen has to offer a
            // field for every item the business sells, including ones never negotiated with this
            // cleaner, which is exactly where an admin needs to type a number.
            return itemTypes.Select(itemType =>
            {
                var price = agreed.FirstOrDefault(p => p.ItemTypeId == itemType.Id);
                return MapToItemPriceDto(itemType, price);
            }).ToList();
        }

        public async Task<List<CleanerItemPriceDto>> SaveItemPricesAsync(Guid cleanerId, SaveCleanerItemPricesDto dto)
        {
            var cleaner = await _cleanerRepository.GetByIdAsync(cleanerId);
            if (cleaner == null) throw AppException.NotFound(ErrorCodes.CleanerNotFound);

            var itemTypes = await _itemTypeRepository.GetAllAsync();
            var knownItemTypeIds = itemTypes.Select(t => t.Id).ToHashSet();

            foreach (var item in dto.Items)
            {
                if (!knownItemTypeIds.Contains(item.ItemTypeId))
                {
                    throw AppException.NotFound(ErrorCodes.ItemTypeNotFound);
                }

                if (item.IronPrice < 0 || item.CleaningPrice < 0 || item.BothPrice < 0)
                {
                    throw new AppException(ErrorCodes.CleanerItemPriceNegative);
                }
            }

            // A row of three zeroes is the admin saying "nothing agreed here", which is the same
            // thing as having no row at all - an item this laundry is simply not paid for.
            // Storing the row would only add noise to the rate card.
            var prices = dto.Items
                .Where(i => i.IronPrice > 0 || i.CleaningPrice > 0 || i.BothPrice > 0)
                .Select(i => new CleanerItemPrice
                {
                    CleanerId = cleanerId,
                    ItemTypeId = i.ItemTypeId,
                    IronPrice = i.IronPrice,
                    CleaningPrice = i.CleaningPrice,
                    BothPrice = i.BothPrice
                })
                .ToList();

            await _cleanerItemPriceRepository.SaveForCleanerAsync(cleanerId, prices);
            return await GetItemPricesAsync(cleanerId);
        }

        private CleanerItemPriceDto MapToItemPriceDto(ItemType itemType, CleanerItemPrice? price)
        {
            return new CleanerItemPriceDto
            {
                ItemTypeId = itemType.Id,
                TypeNameAr = itemType.TypeNameAr,
                TypeNameEn = itemType.TypeNameEn,
                TypeName = BilingualText.Pick(itemType.TypeNameAr, itemType.TypeNameEn, _language.Language),
                CustomerIronPrice = itemType.IronPrice,
                CustomerCleaningPrice = itemType.CleaningPrice,
                CustomerBothPrice = itemType.BothPrice,
                IronPrice = price?.IronPrice ?? 0,
                CleaningPrice = price?.CleaningPrice ?? 0,
                BothPrice = price?.BothPrice ?? 0,
                HasAgreedPrice = price != null
            };
        }

        private CleanerDto MapToDto(Cleaner cleaner)
        {
            return new CleanerDto
            {
                Id = cleaner.Id,
                NameAr = cleaner.NameAr,
                NameEn = cleaner.NameEn,
                Name = BilingualText.Pick(cleaner.NameAr, cleaner.NameEn, _language.Language),
                Note = cleaner.Note,
                CleaningLocation = cleaner.CleaningLocation,
                Latitude = cleaner.Latitude,
                Longitude = cleaner.Longitude,
                CreatedAt = cleaner.CreatedAt
            };
        }
    }
}
