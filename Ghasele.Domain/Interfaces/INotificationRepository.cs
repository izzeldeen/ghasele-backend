using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Domain.Entities;

namespace Ghasele.Domain.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification?> GetByIdAsync(Guid id);

        /// <summary>The account's list when <paramref name="userId"/> is set, otherwise the guest install's.</summary>
        Task<List<Notification>> GetByOwnerAsync(Guid? userId, string? deviceToken);
        Task<List<Notification>> GetUnreadByOwnerAsync(Guid? userId, string? deviceToken);

        /// <summary>Moves a guest install's notifications onto the account that just signed in on it.</summary>
        Task<int> ClaimGuestNotificationsAsync(string deviceToken, Guid userId);

        Task AddAsync(Notification notification);
        Task UpdateAsync(Notification notification);
        Task DeleteAsync(Notification notification);
    }
}
