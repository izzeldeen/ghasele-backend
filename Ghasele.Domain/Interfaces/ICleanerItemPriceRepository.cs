using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ghasele.Domain.Entities;

namespace Ghasele.Domain.Interfaces
{
    public interface ICleanerItemPriceRepository
    {
        /// <summary>Every rate agreed with one cleaner, item type included.</summary>
        Task<List<CleanerItemPrice>> GetByCleanerAsync(Guid cleanerId);

        /// <summary>
        /// Replaces this cleaner's agreed rates with <paramref name="prices"/> in one go: rows
        /// for item types in the list are inserted or updated, and rows for item types absent
        /// from it are removed. The admin screen submits the whole grid, so a partial upsert
        /// would leave rates behind for items the admin had cleared.
        /// </summary>
        Task SaveForCleanerAsync(Guid cleanerId, IEnumerable<CleanerItemPrice> prices);
    }
}
