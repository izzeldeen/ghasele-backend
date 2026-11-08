using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ghasele.Application.Interfaces
{
    public interface INotificationService
    {
        /// <summary>
        /// Sends a push notification, optionally carrying extra data (e.g. a "type" and
        /// "orderId") a client reads to decide where to navigate on tap.
        /// </summary>
        Task SendNotificationAsync(string fcmToken, string title, string body, Dictionary<string, string>? data = null);
    }
}
