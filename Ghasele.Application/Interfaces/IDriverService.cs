using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;

namespace Ghasele.Application.Interfaces
{
    public interface IDriverService
    {
        Task<DriverDto> CreateDriverAsync(CreateDriverDto dto);
        Task<IEnumerable<DriverDto>> GetAllDriversAsync();
        Task<DriverDto?> GetDriverByIdAsync(Guid id);
        Task<DriverDto> UpdateDriverAsync(Guid id, UpdateDriverDto dto);
        Task DeleteDriverAsync(Guid id);
    }
}
