using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;

namespace Ghasele.Application.Interfaces
{
    public interface IUserNotificationService
    {
        Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId);
        Task MarkAsReadAsync(Guid notificationId);
        Task MarkAllAsReadAsync(Guid userId);
        Task CreateNotificationAsync(Guid userId, string title, string body);
        Task DeleteNotificationAsync(Guid notificationId);
    }
}
