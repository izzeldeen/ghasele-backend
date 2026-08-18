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

        public CleanerService(ICleanerRepository cleanerRepository)
        {
            _cleanerRepository = cleanerRepository;
        }

        public async Task<CleanerDto> CreateCleanerAsync(CreateCleanerDto dto)
        {
            var cleaner = new Cleaner
            {
                Name = dto.Name,
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

            if (!string.IsNullOrEmpty(dto.Name)) cleaner.Name = dto.Name;
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

        private static CleanerDto MapToDto(Cleaner cleaner)
        {
            return new CleanerDto
            {
                Id = cleaner.Id,
                Name = cleaner.Name,
                Note = cleaner.Note,
                CleaningLocation = cleaner.CleaningLocation,
                Latitude = cleaner.Latitude,
                Longitude = cleaner.Longitude,
                CreatedAt = cleaner.CreatedAt
            };
        }
    }
}
