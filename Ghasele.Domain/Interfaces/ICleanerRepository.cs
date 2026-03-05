using Ghasele.Domain.Entities;

namespace Ghasele.Domain.Interfaces
{
    public interface ICleanerRepository
    {
        Task<Cleaner> AddAsync(Cleaner cleaner);
        Task<List<Cleaner>> GetAllAsync();
        Task<Cleaner?> GetByIdAsync(Guid id);
        Task UpdateAsync(Cleaner cleaner);
        Task DeleteAsync(Guid id);
    }
}
