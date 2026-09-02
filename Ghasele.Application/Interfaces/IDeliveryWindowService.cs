using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;

namespace Ghasele.Application.Interfaces
{
    public interface IDeliveryWindowService
    {
        Task<List<DeliveryWindowDto>> GetAllAsync();
        Task<DeliveryWindowDto> CreateAsync(CreateDeliveryWindowDto dto);
        Task<DeliveryWindowDto> UpdateAsync(Guid id, UpdateDeliveryWindowDto dto);
        Task DeleteAsync(Guid id);

        /// <summary>Active windows projected across the next <paramref name="days"/> calendar days.</summary>
        Task<List<DeliverySlotDto>> GetUpcomingSlotsAsync(int days);
    }
}
