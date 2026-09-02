using System.Threading.Tasks;
using Ghasele.Application.DTOs;
using Ghasele.Application.Exceptions;
using Ghasele.Application.Interfaces;
using Ghasele.Application.Localization;
using Ghasele.Domain.Interfaces;

namespace Ghasele.Application.Services
{
    public class AppSettingsService : IAppSettingsService
    {
        private readonly IAppSettingsRepository _repository;

        public AppSettingsService(IAppSettingsRepository repository)
        {
            _repository = repository;
        }

        public async Task<AppSettingsDto> GetAsync()
        {
            var settings = await _repository.GetAsync();
            return new AppSettingsDto
            {
                NormalDeliveryPrice = settings.NormalDeliveryPrice,
                ExpressDeliveryPrice = settings.ExpressDeliveryPrice,
                UpdatedAt = settings.UpdatedAt
            };
        }

        public async Task<AppSettingsDto> UpdateAsync(UpdateAppSettingsDto dto)
        {
            if (dto.NormalDeliveryPrice < 0 || dto.ExpressDeliveryPrice < 0)
            {
                throw new AppException(ErrorCodes.DeliveryPriceNegative);
            }

            var settings = await _repository.GetAsync();
            settings.NormalDeliveryPrice = dto.NormalDeliveryPrice;
            settings.ExpressDeliveryPrice = dto.ExpressDeliveryPrice;
            await _repository.UpdateAsync(settings);

            return new AppSettingsDto
            {
                NormalDeliveryPrice = settings.NormalDeliveryPrice,
                ExpressDeliveryPrice = settings.ExpressDeliveryPrice,
                UpdatedAt = settings.UpdatedAt
            };
        }

        public async Task<DeliveryPricingDto> GetDeliveryPricingAsync()
        {
            var settings = await _repository.GetAsync();
            return new DeliveryPricingDto
            {
                NormalDeliveryPrice = settings.NormalDeliveryPrice,
                ExpressDeliveryPrice = settings.ExpressDeliveryPrice
            };
        }
    }
}
