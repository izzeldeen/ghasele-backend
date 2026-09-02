using System.Threading.Tasks;
using Ghasele.Domain.Entities;

namespace Ghasele.Domain.Interfaces
{
    public interface IAppSettingsRepository
    {
        /// <summary>
        /// Returns the settings row, creating it from the seeded defaults if it is
        /// somehow missing, so callers never have to null-check.
        /// </summary>
        Task<AppSettings> GetAsync();

        Task UpdateAsync(AppSettings settings);
    }
}
