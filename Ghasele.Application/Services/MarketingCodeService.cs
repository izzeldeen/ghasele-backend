using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;
using Ghasele.Application.Interfaces;
using Ghasele.Domain.Entities;
using Ghasele.Domain.Interfaces;

namespace Ghasele.Application.Services
{
    public class MarketingCodeService : IMarketingCodeService
    {
        private readonly IMarketingCodeRepository _marketingCodeRepository;

        public MarketingCodeService(IMarketingCodeRepository marketingCodeRepository)
        {
            _marketingCodeRepository = marketingCodeRepository;
        }

        public async Task<MarketingCodeDto?> GetByIdAsync(Guid id)
        {
            var code = await _marketingCodeRepository.GetByIdAsync(id);
            return code != null ? MapToDto(code) : null;
        }

        public async Task<MarketingCodeDto?> GetByCodeAsync(string code)
        {
            var marketingCode = await _marketingCodeRepository.GetByCodeAsync(code);
            return marketingCode != null ? MapToDto(marketingCode) : null;
        }

        public async Task<List<MarketingCodeDto>> GetAllAsync()
        {
            var codes = await _marketingCodeRepository.GetAllAsync();
            return codes.Select(MapToDto).ToList();
        }

        public async Task<MarketingCodeDto> CreateAsync(CreateMarketingCodeDto dto)
        {
            var marketingCode = new MarketingCode
            {
                Code = dto.Code,
                DiscountPercentage = dto.DiscountPercentage,
                SharePercentage = dto.SharePercentage,
                MarketerName = dto.MarketerName
            };

            await _marketingCodeRepository.AddAsync(marketingCode);
            return MapToDto(marketingCode);
        }

        public async Task<MarketingCodeDto> UpdateAsync(Guid id, UpdateMarketingCodeDto dto)
        {
            var code = await _marketingCodeRepository.GetByIdAsync(id);
            if (code == null) throw new Exception("Marketing code not found");

            if (dto.DiscountPercentage.HasValue) code.DiscountPercentage = dto.DiscountPercentage.Value;
            if (dto.SharePercentage.HasValue) code.SharePercentage = dto.SharePercentage.Value;
            if (dto.MarketerName != null) code.MarketerName = dto.MarketerName;
            if (dto.IsActive.HasValue) code.IsActive = dto.IsActive.Value;

            await _marketingCodeRepository.UpdateAsync(code);
            return MapToDto(code);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _marketingCodeRepository.DeleteAsync(id);
        }

        private MarketingCodeDto MapToDto(MarketingCode code)
        {
            return new MarketingCodeDto
            {
                Id = code.Id,
                Code = code.Code,
                DiscountPercentage = code.DiscountPercentage,
                SharePercentage = code.SharePercentage,
                MarketerName = code.MarketerName,
                IsActive = code.IsActive,
                CreatedAt = code.CreatedAt
            };
        }
    }
}
