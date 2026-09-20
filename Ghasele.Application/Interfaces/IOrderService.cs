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

        /// <summary>
        /// Cancels the customer's own order, while it is still theirs to cancel.
        /// </summary>
        /// <remarks>
        /// Ownership is proved by exactly one of the two arguments - the signed-in caller's
        /// id, or the device token a guest placed the order with - and is checked here rather
        /// than at the controller, so both entry points enforce the same rule.
        /// </remarks>
        Task<OrderDto> CancelOrderAsync(Guid id, Guid? callerId, string? deviceToken);

        /// <summary>Moves the customer's own order to a different collection slot.</summary>
        Task<OrderDto> RescheduleOrderAsync(Guid id, RescheduleOrderDto dto, Guid? callerId, string? deviceToken);
        Task<OrderDto> DeleteOrderItemAsync(Guid orderId, Guid itemId);
        Task DeleteOrderAsync(Guid id);
    }
}
