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
        private readonly IUserRepository _userRepository;
        private readonly ICurrentLanguageProvider _language;

        public DriverService(IDriverRepository driverRepository, IUserRepository userRepository, ICurrentLanguageProvider language)
        {
            _driverRepository = driverRepository;
            _userRepository = userRepository;
            _language = language;
        }

        public async Task<DriverDto> CreateDriverAsync(CreateDriverDto dto)
        {
            var existingUser = await _userRepository.GetByPhoneNumberAsync(dto.PhoneNumber);
            if (existingUser != null)
            {
                throw new AppException(ErrorCodes.PhoneAlreadyExists, 409);
            }

            // Every driver gets a login as part of being created - there is no separate
            // "invite" step, so the admin hands the driver their phone number and password.
            var user = new User
            {
                Username = dto.PhoneNumber,
                PhoneNumber = dto.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                // Not request-language-dependent: this is the account's own name of record,
                // not something displayed back per-viewer, so it shouldn't shift with whichever
                // language the admin happened to be using when they created the driver.
                FullName = BilingualText.Pick(dto.NameAr, dto.NameEn, "en"),
                Role = UserRole.Driver,
                IsPhoneVerified = true
            };
            await _userRepository.AddAsync(user);

            var driver = new Driver
            {
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                PhoneNumber = dto.PhoneNumber,
                Note = dto.Note,
                CreatedAt = DateTime.UtcNow,
                UserId = user.Id
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

            if (!string.IsNullOrEmpty(dto.NameAr)) driver.NameAr = dto.NameAr;
            if (!string.IsNullOrEmpty(dto.NameEn)) driver.NameEn = dto.NameEn;
            if (!string.IsNullOrEmpty(dto.PhoneNumber)) driver.PhoneNumber = dto.PhoneNumber;
            if (dto.Note != null) driver.Note = dto.Note;

            await _driverRepository.UpdateAsync(driver);
            return MapToDto(driver);
        }

        public async Task DeleteDriverAsync(Guid id)
        {
            await _driverRepository.DeleteAsync(id);
        }

        private DriverDto MapToDto(Driver driver)
        {
            return new DriverDto
            {
                Id = driver.Id,
                NameAr = driver.NameAr,
                NameEn = driver.NameEn,
                Name = BilingualText.Pick(driver.NameAr, driver.NameEn, _language.Language),
                PhoneNumber = driver.PhoneNumber,
                Note = driver.Note,
                CreatedAt = driver.CreatedAt,
                UserId = driver.UserId
            };
        }
    }
}
