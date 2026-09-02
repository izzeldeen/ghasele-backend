using System;
using System.Threading.Tasks;
using Ghasele.Application.Interfaces;

namespace Ghasele.Infrastructure.Services
{
    public class MockWhatsAppService : IWhatsAppService
    {
        public Task SendOtpAsync(string phoneNumber, string code)
        {
            // Simulate sending the verify_code_1 template by logging to the console.
            Console.WriteLine("=============================================");
            Console.WriteLine($"[MOCK WHATSAPP] Template: verify_code_1 -> {phoneNumber}");
            Console.WriteLine($"[MOCK WHATSAPP] Code: {code}");
            Console.WriteLine("=============================================");

            return Task.CompletedTask;
        }
    }
}
