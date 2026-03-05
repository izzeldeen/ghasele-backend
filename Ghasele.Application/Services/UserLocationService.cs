using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;
using Ghasele.Application.Interfaces;
using Ghasele.Domain.Entities;
using Ghasele.Domain.Interfaces;

namespace Ghasele.Application.Services
{
    public class UserLocationService : IUserLocationService
    {
        private readonly IUserLocationRepository _userLocationRepository;

        public UserLocationService(IUserLocationRepository userLocationRepository)
        {
            _userLocationRepository = userLocationRepository;
        }

        public async Task<UserLocationDto> AddLocationAsync(CreateUserLocationDto dto)
        {
            var location = new UserLocation
            {
                UserId = dto.UserId,
                Name = dto.Name,
                Lat = dto.Lat,
                Long = dto.Long
            };

            await _userLocationRepository.AddAsync(location);

            return new UserLocationDto
            {
                Id = location.Id,
                Name = location.Name,
                Lat = location.Lat,
                Long = location.Long
            };
        }

        public async Task<List<UserLocationDto>> GetUserLocationsAsync(Guid userId)
        {
            var locations = await _userLocationRepository.GetByUserIdAsync(userId);
            return locations.Select(l => new UserLocationDto
            {
                Id = l.Id,
                Name = l.Name,
                Lat = l.Lat,
                Long = l.Long
            }).ToList();
        }

        public async Task DeleteLocationAsync(Guid id)
        {
            await _userLocationRepository.DeleteAsync(id);
        }
    }
}
