using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ghasele.Domain.Entities;
using Ghasele.Domain.Interfaces;
using Ghasele.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ghasele.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Order> AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<List<Order>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Trip).ThenInclude(t => t!.Driver)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Guest orders placed from one device, newest first. Restricted to IsGuest rows so that
        /// if a device token ever ends up on an order that was later claimed by an account, the
        /// anonymous listing cannot read it back.
        /// </summary>
        public async Task<List<Order>> GetByDeviceTokenAsync(string deviceToken, int page = 1, int pageSize = 20)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Trip).ThenInclude(t => t!.Driver)
                .Where(o => o.IsGuest && o.DeviceToken == deviceToken)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> ClaimGuestOrdersAsync(string deviceToken, Guid userId)
        {
            // Keyed on an absent owner rather than on IsGuest: UserId is what every other query
            // means by "whose order is this", and it is the column that must not be overwritten.
            // An order already claimed - by this account or another one on the same handset - has
            // an owner, so it is excluded and a second sign-in cannot take it.
            //
            // One UPDATE rather than load-modify-save: this runs inside a sign-in, where the whole
            // point is that it either moves every matching order or none, and cannot half-finish
            // while the customer is waiting.
            return await _context.Orders
                .Where(o => o.UserId == null && o.DeviceToken == deviceToken)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(o => o.UserId, userId)
                    // The order now belongs to an account, so it stops being a guest order: it
                    // leaves the device-token listing and joins the customer's own, and pushes
                    // start going to the account's token (see Order.ResolvePushToken).
                    .SetProperty(o => o.IsGuest, false));
        }

        public async Task<int> UpdateGuestFcmTokenAsync(string deviceToken, string fcmToken)
        {
            // Delivered and cancelled orders produce no further notifications, so they are left
            // alone: this runs on every launch and there is no reason to rewrite history.
            return await _context.Orders
                .Where(o => o.IsGuest
                            && o.DeviceToken == deviceToken
                            && o.FcmToken != fcmToken
                            && o.Status != OrderStatus.Delivered
                            && o.Status != OrderStatus.Cancelled)
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.FcmToken, fcmToken));
        }

        public async Task<Dictionary<(Guid WindowId, DateOnly Date), int>> GetBookedCountsAsync(DateOnly from)
        {
            // Cancelled orders free their seat again - holding capacity for an order nobody
            // is going to collect would quietly shrink the schedule.
            var rows = await _context.Orders
                .Where(o => o.DeliveryWindowId != null
                            && o.ScheduledDate != null
                            && o.ScheduledDate >= from
                            && o.Status != OrderStatus.Cancelled)
                .GroupBy(o => new { WindowId = o.DeliveryWindowId!.Value, Date = o.ScheduledDate!.Value })
                .Select(g => new { g.Key.WindowId, g.Key.Date, Count = g.Count() })
                .ToListAsync();

            return rows.ToDictionary(r => (r.WindowId, r.Date), r => r.Count);
        }

        public Task<int> CountBookedAsync(Guid windowId, DateOnly date) =>
            _context.Orders.CountAsync(o => o.DeliveryWindowId == windowId
                                            && o.ScheduledDate == date
                                            && o.Status != OrderStatus.Cancelled);

        public async Task<Order?> GetByIdAsync(Guid id)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Trip).ThenInclude(t => t!.Cleaner)
                .Include(o => o.Trip).ThenInclude(t => t!.Driver)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);
        }
      public Order Update(Order order)
{
    _context.Orders.Update(order);
    _context.SaveChanges();
    return order;
}

        public async Task UpdateAsync(Order order)
        {
            // Ensure navigation properties (User, Trip) are not marked as modified
            // Only the Order entity itself and its Items collection should be updated
            if (order.User != null)
            {
                _context.Entry(order.User).State = EntityState.Unchanged;
            }
            if (order.Trip != null)
            {
                _context.Entry(order.Trip).State = EntityState.Unchanged;
            }
            
            await _context.SaveChangesAsync();
        }

        public async Task<Order?> GetByIdForUpdateAsync(Guid id)
        {
            // Load order with only Items for updates to avoid tracking conflicts with User/Trip
            return await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<Order>> GetAllAsync(OrderStatus? status = null, string? searchTerm = null)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Trip).ThenInclude(t => t!.Cleaner)
                .Include(o => o.Trip).ThenInclude(t => t!.Driver)
                .Include(o => o.Items)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(o => 
                    o.ReferenceNumber.ToLower().Contains(lowerSearchTerm) || 
                    (o.User != null && o.User.FullName.ToLower().Contains(lowerSearchTerm))
                );
            }

            return await query
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> HasPendingOrderAsync(Guid userId)
        {
            return await _context.Orders
                .AnyAsync(o => o.UserId == userId && o.Status == OrderStatus.PendingCollection);
        }

        public async Task DeleteAsync(Guid id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddItemsAsync(IEnumerable<OrderItem> items)
        {
            await _context.Set<OrderItem>().AddRangeAsync(items);
            await _context.SaveChangesAsync();
        }

        public async Task<OrderItem?> GetItemByIdAsync(Guid itemId)
        {
            return await _context.Set<OrderItem>().FirstOrDefaultAsync(i => i.Id == itemId);
        }

        public async Task DeleteItemAsync(OrderItem item)
        {
            _context.Set<OrderItem>().Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}
