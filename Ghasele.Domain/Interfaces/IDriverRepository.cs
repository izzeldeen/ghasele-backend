using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Domain.Entities;

namespace Ghasele.Domain.Interfaces
{
    public interface IDriverRepository
    {
        Task<Driver> AddAsync(Driver driver);
        Task<List<Driver>> GetAllAsync();
        Task<Driver?> GetByIdAsync(Guid id);
        Task UpdateAsync(Driver driver);
        Task DeleteAsync(Guid id);
    }
}
