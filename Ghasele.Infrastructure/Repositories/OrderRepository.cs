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

        public async Task<List<Order>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Trip).ThenInclude(t => t!.Driver)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

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
                .AnyAsync(o => o.UserId == userId && o.Status == OrderStatus.Pending);
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
    }
}
