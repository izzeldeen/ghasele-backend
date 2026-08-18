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
    public class DriverService : IDriverService
    {
        private readonly IDriverRepository _driverRepository;

        public DriverService(IDriverRepository driverRepository)
        {
            _driverRepository = driverRepository;
        }

        public async Task<DriverDto> CreateDriverAsync(CreateDriverDto dto)
        {
            var driver = new Driver
            {
                Name = dto.Name,
                PhoneNumber = dto.PhoneNumber,
                Note = dto.Note,
                CreatedAt = DateTime.UtcNow
            };

            await _driverRepository.AddAsync(driver);
            return MapToDto(driver);
        }

        public async Task<IEnumerable<DriverDto>> GetAllDriversAsync()
        {
            var drivers = await _driverRepository.GetAllAsync();
            return drivers.Select(MapToDto);
        }

        public async Task<DriverDto?> GetDriverByIdAsync(Guid id)
        {
            var driver = await _driverRepository.GetByIdAsync(id);
            return driver != null ? MapToDto(driver) : null;
        }

        public async Task<DriverDto> UpdateDriverAsync(Guid id, UpdateDriverDto dto)
        {
            var driver = await _driverRepository.GetByIdAsync(id);
            if (driver == null) throw AppException.NotFound(ErrorCodes.DriverNotFound);

            if (!string.IsNullOrEmpty(dto.Name)) driver.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.PhoneNumber)) driver.PhoneNumber = dto.PhoneNumber;
            if (dto.Note != null) driver.Note = dto.Note;

            await _driverRepository.UpdateAsync(driver);
            return MapToDto(driver);
        }

        public async Task DeleteDriverAsync(Guid id)
        {
            await _driverRepository.DeleteAsync(id);
        }

        private static DriverDto MapToDto(Driver driver)
        {
            return new DriverDto
            {
                Id = driver.Id,
                Name = driver.Name,
                PhoneNumber = driver.PhoneNumber,
                Note = driver.Note,
                CreatedAt = driver.CreatedAt
            };
        }
    }
}
