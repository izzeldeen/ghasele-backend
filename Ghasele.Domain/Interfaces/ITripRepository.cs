using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Domain.Entities;

namespace Ghasele.Domain.Interfaces
{
    public interface ITripRepository
    {
        Task<Trip> AddAsync(Trip trip);
        Task<Trip?> GetByIdAsync(Guid id);
        Task<List<Trip>> GetAllAsync();
        Task<List<Trip>> GetByDriverIdAsync(Guid driverId);
        Task UpdateAsync(Trip trip);
        Task DeleteAsync(Guid id);
    }
}
