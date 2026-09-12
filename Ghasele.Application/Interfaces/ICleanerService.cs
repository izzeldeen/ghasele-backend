using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;

namespace Ghasele.Application.Interfaces
{
    public interface ICleanerService
    {
        Task<CleanerDto> CreateCleanerAsync(CreateCleanerDto dto);
        Task<List<CleanerDto>> GetAllCleanersAsync();
        Task<CleanerDto?> GetCleanerByIdAsync(Guid id);
        Task<CleanerDto> UpdateCleanerAsync(Guid id, UpdateCleanerDto dto);
        Task DeleteCleanerAsync(Guid id);

        /// <summary>
        /// This cleaner's rate card: every active item type, with the customer prices from the
        /// item type and whatever rate has been agreed with this cleaner (0 where none has).
        /// </summary>
        Task<List<CleanerItemPriceDto>> GetItemPricesAsync(Guid cleanerId);

        /// <summary>Replaces this cleaner's agreed rates with the submitted grid.</summary>
        Task<List<CleanerItemPriceDto>> SaveItemPricesAsync(Guid cleanerId, SaveCleanerItemPricesDto dto);
    }
}
