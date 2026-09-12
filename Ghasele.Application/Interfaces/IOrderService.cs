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
        Task<List<OrderDto>> GetGuestOrdersAsync(string deviceToken, int page = 1, int pageSize = 20);

        /// <summary>
        /// Re-points this device's open guest orders at the app's current FCM token. Returns how
        /// many orders were updated.
        /// </summary>
        Task<int> UpdateGuestFcmTokenAsync(string deviceToken, string fcmToken);

        /// <summary>
        /// Moves the orders a guest placed on this device onto the account they just signed in
        /// with. Returns how many were claimed; zero when there is nothing to claim.
        /// </summary>
        Task<int> ClaimGuestOrdersAsync(Guid userId, string? deviceToken);
        Task<List<OrderDto>> GetAllOrdersAsync(string? status = null, string? searchTerm = null);
        Task<OrderDto?> GetOrderByIdAsync(Guid id);
        Task<OrderDto> AddItemsToOrderAsync(Guid orderId, AddOrderItemsDto dto);
        Task<OrderDto> UpdateOrderAsync(Guid id, UpdateOrderDto dto);
        Task<OrderDto> DeleteOrderItemAsync(Guid orderId, Guid itemId);
        Task DeleteOrderAsync(Guid id);
    }
}
