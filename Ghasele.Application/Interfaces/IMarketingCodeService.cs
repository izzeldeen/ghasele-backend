using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;

namespace Ghasele.Application.Interfaces
{
    public interface IMarketingCodeService
    {
        Task<MarketingCodeDto?> GetByIdAsync(Guid id);
        Task<MarketingCodeDto?> GetByCodeAsync(string code);
        Task<List<MarketingCodeDto>> GetAllAsync();
        Task<MarketingCodeDto> CreateAsync(CreateMarketingCodeDto dto);
        Task<MarketingCodeDto> UpdateAsync(Guid id, UpdateMarketingCodeDto dto);
        Task DeleteAsync(Guid id);
    }
}
