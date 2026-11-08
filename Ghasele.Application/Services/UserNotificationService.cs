using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;
using Ghasele.Application.Interfaces;
using Ghasele.Domain.Entities;
using Ghasele.Domain.Interfaces;

namespace Ghasele.Application.Services
{
    public class UserNotificationService : IUserNotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public UserNotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<List<NotificationDto>> GetNotificationsAsync(Guid? userId, string? deviceToken)
        {
            var notifications = await _notificationRepository.GetByOwnerAsync(userId, deviceToken);
            return notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Body = n.Body,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                OrderId = n.OrderId
            }).ToList();
        }

        /// <summary>
        /// True when <paramref name="notification"/> is the caller's. The same rule as the list:
        /// an account owns what carries its id, a guest only what carries its device token and
        /// no account - so an id seen once cannot be used to touch someone else's entry.
        /// </summary>
        private static bool IsOwnedBy(Notification notification, Guid? userId, string? deviceToken)
        {
            if (userId is Guid id) return notification.UserId == id;
            return notification.UserId == null
                && !string.IsNullOrWhiteSpace(deviceToken)
                && string.Equals(notification.DeviceToken, deviceToken, StringComparison.Ordinal);
        }

        public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid? userId, string? deviceToken)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null || !IsOwnedBy(notification, userId, deviceToken)) return false;

            notification.IsRead = true;
            await _notificationRepository.UpdateAsync(notification);
            return true;
        }

        public async Task MarkAllAsReadAsync(Guid? userId, string? deviceToken)
        {
            var notifications = await _notificationRepository.GetUnreadByOwnerAsync(userId, deviceToken);

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                await _notificationRepository.UpdateAsync(notification);
            }
        }

        public async Task CreateNotificationAsync(Guid userId, string title, string body, Guid? orderId = null)
        {
            await AddAsync(new Notification { UserId = userId }, title, body, orderId);
        }

        public async Task CreateForOrderAsync(Order order, string title, string body, Guid? orderId = null)
        {
            if (order.UserId is Guid userId)
            {
                await CreateNotificationAsync(userId, title, body, orderId);
            }
            else if (!string.IsNullOrWhiteSpace(order.DeviceToken))
            {
                await AddAsync(new Notification { DeviceToken = order.DeviceToken }, title, body, orderId);
            }
        }

        private Task AddAsync(Notification notification, string title, string body, Guid? orderId)
        {
            notification.Title = title;
            notification.Body = body;
            notification.OrderId = orderId;
            notification.CreatedAt = DateTime.UtcNow;
            notification.IsRead = false;
            return _notificationRepository.AddAsync(notification);
        }

        public async Task<int> ClaimGuestNotificationsAsync(Guid userId, string? deviceToken)
        {
            if (string.IsNullOrWhiteSpace(deviceToken)) return 0;
            return await _notificationRepository.ClaimGuestNotificationsAsync(deviceToken.Trim(), userId);
        }

        public async Task<bool> DeleteNotificationAsync(Guid notificationId, Guid? userId, string? deviceToken)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null || !IsOwnedBy(notification, userId, deviceToken)) return false;

            await _notificationRepository.DeleteAsync(notification);
            return true;
        }
    }
}
