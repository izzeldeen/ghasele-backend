using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Domain.Entities;

namespace Ghasele.Domain.Interfaces
{
    public interface IMarketingCodeRepository
    {
        Task<MarketingCode?> GetByIdAsync(Guid id);
        Task<MarketingCode?> GetByCodeAsync(string code);
        Task<List<MarketingCode>> GetAllAsync();
        Task AddAsync(MarketingCode marketingCode);
        Task UpdateAsync(MarketingCode marketingCode);
        Task DeleteAsync(Guid id);
    }
}
