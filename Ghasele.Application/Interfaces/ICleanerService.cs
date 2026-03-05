using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;

namespace Ghasele.Application.Interfaces
{
    public interface ICleanerService
    {
        Task<CleanerDto> CreateCleanerAsync(CreateCleanerDto dto);
        Task<List<CleanerDto>> GetAllCleanersAsync();
        Task<CleanerDto?> GetCleanerByIdAsync(Guid id);
        Task<CleanerDto> UpdateCleanerAsync(Guid id, UpdateCleanerDto dto);
        Task DeleteCleanerAsync(Guid id);
    }
}
