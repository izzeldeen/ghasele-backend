using System.Threading.Tasks;
using Ghasele.Application.Interfaces;
using WhatsappBusiness.CloudApi.Interfaces;
using WhatsappBusiness.CloudApi.Messages.Requests;

namespace Ghasele.Infrastructure.Services
{
    public class WhatsAppCloudApiService : IWhatsAppService
    {
        private readonly IWhatsAppBusinessClient _whatsAppBusinessClient;

        public WhatsAppCloudApiService(IWhatsAppBusinessClient whatsAppBusinessClient)
        {
            _whatsAppBusinessClient = whatsAppBusinessClient;
        }

        public async Task SendMessageAsync(string phoneNumber, string message)
        {
            var textMessageRequest = new TextMessageRequest
            {
                To = phoneNumber,
                Text = new WhatsAppText
                {
                    Body = message,
                    PreviewUrl = false
                }
            };

            await _whatsAppBusinessClient.SendTextMessageAsync(textMessageRequest);
        }
    }
}
