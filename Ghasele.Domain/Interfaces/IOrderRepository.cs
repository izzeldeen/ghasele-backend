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
        Task<List<Order>> GetByDeviceTokenAsync(string deviceToken, int page = 1, int pageSize = 20);

        /// <summary>
        /// Points this device's still-open guest orders at a new FCM token, returning how many
        /// rows were updated. Called when FCM hands the app a different token, which it does on
        /// reinstall, restore and its own schedule - without this the token stamped at checkout
        /// goes stale and the pushes silently stop arriving.
        /// </summary>
        Task<int> UpdateGuestFcmTokenAsync(string deviceToken, string fcmToken);

        /// <summary>
        /// Hands every unowned order placed from <paramref name="deviceToken"/> to
        /// <paramref name="userId"/>. Returns how many moved.
        /// </summary>
        Task<int> ClaimGuestOrdersAsync(string deviceToken, Guid userId);
        Task<List<Order>> GetAllAsync(OrderStatus? status = null, string? searchTerm = null);
        Task<Order?> GetByIdAsync(Guid id);
        Task<Order?> GetByIdForUpdateAsync(Guid id);
        Task UpdateAsync(Order order);
        Task<bool> HasPendingOrderAsync(Guid userId);

        /// <summary>
        /// How many orders are booked into each (window, date) pair from <paramref name="from"/>
        /// onwards, keyed by the pair. Returned in one query rather than per slot: the customer
        /// app asks for a week of slots at once, which would otherwise be dozens of round trips.
        /// </summary>
        Task<Dictionary<(Guid WindowId, DateOnly Date), int>> GetBookedCountsAsync(DateOnly from);

        /// <summary>Orders already booked into one window on one date.</summary>
        Task<int> CountBookedAsync(Guid windowId, DateOnly date);
        Order Update(Order order);
        Task DeleteAsync(Guid id);
        Task AddItemsAsync(IEnumerable<OrderItem> items);
        Task<OrderItem?> GetItemByIdAsync(Guid itemId);
        Task DeleteItemAsync(OrderItem item);
    }
}
