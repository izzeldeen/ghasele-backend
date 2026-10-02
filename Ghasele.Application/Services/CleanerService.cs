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
        private readonly ICurrentLanguageProvider _language;

        public CleanerService(ICleanerRepository cleanerRepository, ICurrentLanguageProvider language)
        {
            _cleanerRepository = cleanerRepository;
            _language = language;
        }

        /// <summary>The share this laundry keeps when none was given.</summary>
        /// <remarks>
        /// A laundry with no share recorded would cost us nothing on paper, making every
        /// order they handle look like pure margin. Half is the standard agreement, so it
        /// is the safer thing to assume than zero.
        /// </remarks>
        private const decimal DefaultSharePercentage = 50m;

        /// <summary>Refuses a share that is not a percentage.</summary>
        private static void EnsureShareValid(decimal? share)
        {
            if (share.HasValue && (share.Value < 0 || share.Value > 100))
            {
                throw new AppException(ErrorCodes.CleanerSharePercentageInvalid);
            }
        }

        public async Task<CleanerDto> CreateCleanerAsync(CreateCleanerDto dto)
        {
            EnsureShareValid(dto.SharePercentage);

            var cleaner = new Cleaner
            {
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                Note = dto.Note,
                CleaningLocation = dto.CleaningLocation,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                SharePercentage = dto.SharePercentage ?? DefaultSharePercentage,
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
            EnsureShareValid(dto.SharePercentage);

            var cleaner = await _cleanerRepository.GetByIdAsync(id);
            if (cleaner == null) throw AppException.NotFound(ErrorCodes.CleanerNotFound);

            if (!string.IsNullOrEmpty(dto.NameAr)) cleaner.NameAr = dto.NameAr;
            if (!string.IsNullOrEmpty(dto.NameEn)) cleaner.NameEn = dto.NameEn;
            if (dto.Note != null) cleaner.Note = dto.Note;
            if (dto.CleaningLocation != null) cleaner.CleaningLocation = dto.CleaningLocation;
            if (dto.Latitude.HasValue) cleaner.Latitude = dto.Latitude.Value;
            if (dto.Longitude.HasValue) cleaner.Longitude = dto.Longitude.Value;
            if (dto.SharePercentage.HasValue) cleaner.SharePercentage = dto.SharePercentage.Value;

            await _cleanerRepository.UpdateAsync(cleaner);
            return MapToDto(cleaner);
        }

        public async Task DeleteCleanerAsync(Guid id)
        {
            await _cleanerRepository.DeleteAsync(id);
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
                SharePercentage = cleaner.SharePercentage,
                CreatedAt = cleaner.CreatedAt
            };
        }
    }
}
