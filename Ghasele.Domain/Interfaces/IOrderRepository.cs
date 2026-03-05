using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Domain.Entities;

namespace Ghasele.Domain.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> AddAsync(Order order);
        Task<List<Order>> GetByUserIdAsync(Guid userId);
        Task<List<Order>> GetAllAsync(OrderStatus? status = null, string? searchTerm = null);
        Task<Order?> GetByIdAsync(Guid id);
        Task<Order?> GetByIdForUpdateAsync(Guid id);
        Task UpdateAsync(Order order);
        Task<bool> HasPendingOrderAsync(Guid userId);
        Order Update(Order order);
        Task DeleteAsync(Guid id);
        Task AddItemsAsync(IEnumerable<OrderItem> items);
    }
}
