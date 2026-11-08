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
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationDbContext _context;

        public NotificationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            return await _context.Notifications.FindAsync(id);
        }

        public async Task<List<Notification>> GetByOwnerAsync(Guid? userId, string? deviceToken)
        {
            return await OwnedBy(userId, deviceToken)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetUnreadByOwnerAsync(Guid? userId, string? deviceToken)
        {
            return await OwnedBy(userId, deviceToken)
                .Where(n => !n.IsRead)
                .ToListAsync();
        }

        /// <summary>
        /// An account's notifications when there is a user, otherwise the guest install's.
        /// Never both: a signed-in caller's device token is not a second key into the table,
        /// or anyone who had once used the handset as a guest could read the list.
        /// </summary>
        private IQueryable<Notification> OwnedBy(Guid? userId, string? deviceToken)
        {
            if (userId is Guid id)
            {
                return _context.Notifications.Where(n => n.UserId == id);
            }
            if (!string.IsNullOrWhiteSpace(deviceToken))
            {
                return _context.Notifications.Where(n => n.UserId == null && n.DeviceToken == deviceToken);
            }
            return _context.Notifications.Where(n => false);
        }

        public async Task<int> ClaimGuestNotificationsAsync(string deviceToken, Guid userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == null && n.DeviceToken == deviceToken)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.UserId, userId)
                    .SetProperty(n => n.DeviceToken, (string?)null));
        }

        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Notification notification)
        {
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }
    }
}
