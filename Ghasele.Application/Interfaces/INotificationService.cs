using System.Threading.Tasks;

namespace Ghasele.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string fcmToken, string title, string body);
    }
}
