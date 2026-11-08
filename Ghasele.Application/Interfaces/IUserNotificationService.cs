using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;
using Ghasele.Domain.Entities;

namespace Ghasele.Application.Interfaces
{
    /// <summary>
    /// The customer's in-app notification list. Every call names its owner as a user id for a
    /// signed-in customer, or a device token for a guest; the user id wins when both are given.
    /// </summary>
    public interface IUserNotificationService
    {
        Task<List<NotificationDto>> GetNotificationsAsync(Guid? userId, string? deviceToken);

        /// <returns>False when the notification does not exist or belongs to someone else.</returns>
        Task<bool> MarkAsReadAsync(Guid notificationId, Guid? userId, string? deviceToken);
        Task MarkAllAsReadAsync(Guid? userId, string? deviceToken);

        /// <returns>False when the notification does not exist or belongs to someone else.</returns>
        Task<bool> DeleteNotificationAsync(Guid notificationId, Guid? userId, string? deviceToken);

        Task CreateNotificationAsync(Guid userId, string title, string body, Guid? orderId = null);

        /// <summary>
        /// Files a notification under whoever placed <paramref name="order"/>: their account, or
        /// for a guest order the device it was placed from. Does nothing for an order with
        /// neither.
        /// </summary>
        Task CreateForOrderAsync(Order order, string title, string body, Guid? orderId = null);

        /// <summary>Moves a guest install's notifications onto the account that just signed in on it.</summary>
        Task<int> ClaimGuestNotificationsAsync(Guid userId, string? deviceToken);
    }
}
