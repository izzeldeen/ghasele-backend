using System;
using System.Threading.Tasks;
using Ghasele.Application.Interfaces;

namespace Ghasele.Infrastructure.Services
{
    public class MockWhatsAppService : IWhatsAppService
    {
        public Task SendMessageAsync(string phoneNumber, string message)
        {
            // Simulate sending a WhatsApp message by logging to the console
            Console.WriteLine("=============================================");
            Console.WriteLine($"[MOCK WHATSAPP] Sending to: {phoneNumber}");
            Console.WriteLine($"[MOCK WHATSAPP] Message: {message}");
            Console.WriteLine("=============================================");
            
            return Task.CompletedTask;
        }
    }
}
