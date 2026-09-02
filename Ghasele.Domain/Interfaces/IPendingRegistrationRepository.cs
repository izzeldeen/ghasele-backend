using System;
using System.Threading.Tasks;
using Ghasele.Domain.Entities;

namespace Ghasele.Domain.Interfaces
{
    public interface IPendingRegistrationRepository
    {
        Task<PendingRegistration?> GetByPhoneNumberAsync(string phoneNumber);
        Task AddAsync(PendingRegistration pendingRegistration);
        Task UpdateAsync(PendingRegistration pendingRegistration);
        Task DeleteAsync(Guid id);
    }
}
