using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Domain.Entities;

namespace Ghasele.Domain.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> AddAsync(Order order);
        Task<List<Order>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<List<Order>> GetAllAsync(OrderStatus? status = null, string? searchTerm = null);
        Task<Order?> GetByIdAsync(Guid id);
        Task<Order?> GetByIdForUpdateAsync(Guid id);
        Task UpdateAsync(Order order);
        Task<bool> HasPendingOrderAsync(Guid userId);
        Order Update(Order order);
        Task DeleteAsync(Guid id);
        Task AddItemsAsync(IEnumerable<OrderItem> items);
        Task<OrderItem?> GetItemByIdAsync(Guid itemId);
        Task DeleteItemAsync(OrderItem item);
    }
}
