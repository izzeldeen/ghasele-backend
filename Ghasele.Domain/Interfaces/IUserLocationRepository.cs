using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Domain.Entities;

namespace Ghasele.Domain.Interfaces
{
    public interface IUserLocationRepository
    {
        Task<UserLocation> AddAsync(UserLocation location);
        Task<List<UserLocation>> GetByUserIdAsync(Guid userId);
        Task DeleteAsync(Guid id);
    }
}
