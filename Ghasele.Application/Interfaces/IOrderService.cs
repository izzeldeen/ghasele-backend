using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;

namespace Ghasele.Application.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
        Task<List<OrderDto>> GetUserOrdersAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<List<OrderDto>> GetAllOrdersAsync(string? status = null, string? searchTerm = null);
        Task<OrderDto?> GetOrderByIdAsync(Guid id);
        Task<OrderDto> AddItemsToOrderAsync(Guid orderId, AddOrderItemsDto dto);
        Task<OrderDto> UpdateOrderAsync(Guid id, UpdateOrderDto dto);
        Task<OrderDto> DeleteOrderItemAsync(Guid orderId, Guid itemId);
        Task DeleteOrderAsync(Guid id);
    }
}
