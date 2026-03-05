using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;

namespace Ghasele.Application.Interfaces
{
    public interface IUserLocationService
    {
        Task<UserLocationDto> AddLocationAsync(CreateUserLocationDto dto);
        Task<List<UserLocationDto>> GetUserLocationsAsync(Guid userId);
        Task DeleteLocationAsync(Guid id);
    }
}
