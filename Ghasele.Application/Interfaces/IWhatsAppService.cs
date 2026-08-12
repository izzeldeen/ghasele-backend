using System.Threading.Tasks;

namespace Ghasele.Application.Interfaces
{
    public interface IWhatsAppService
    {
        Task SendMessageAsync(string phoneNumber, string message);
    }
}
