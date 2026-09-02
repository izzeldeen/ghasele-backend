using System.Threading.Tasks;
using Ghasele.Application.DTOs;

namespace Ghasele.Application.Interfaces
{
    public interface IAppSettingsService
    {
        Task<AppSettingsDto> GetAsync();
        Task<AppSettingsDto> UpdateAsync(UpdateAppSettingsDto dto);

        /// <summary>Prices only, for the customer app's order-type picker.</summary>
        Task<DeliveryPricingDto> GetDeliveryPricingAsync();
    }
}
