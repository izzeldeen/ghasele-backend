using System.Threading.Tasks;

namespace Ghasele.Application.Interfaces
{
    public interface IWhatsAppService
    {
        /// <summary>
        /// Sends a one-time verification code via the approved WhatsApp template
        /// (<c>verify_code_1</c>), whose single body parameter <c>{{code}}</c> is the 6-digit code.
        /// A template is required because a cold OTP goes to a number that has not messaged the
        /// business in the last 24 hours.
        /// </summary>
        Task SendOtpAsync(string phoneNumber, string code);
    }
}
