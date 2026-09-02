using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Domain.Entities;

namespace Ghasele.Domain.Interfaces
{
    public interface IDeliveryWindowRepository
    {
        Task<List<DeliveryWindow>> GetAllAsync();
        Task<List<DeliveryWindow>> GetActiveAsync();
        Task<DeliveryWindow?> GetByIdAsync(Guid id);
        Task<DeliveryWindow> AddAsync(DeliveryWindow window);
        Task UpdateAsync(DeliveryWindow window);
        Task DeleteAsync(Guid id);
    }
}
