using System;
using System.Threading.Tasks;
using Ghasele.Domain.Entities;
using Ghasele.Domain.Interfaces;
using Ghasele.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ghasele.Infrastructure.Repositories
{
    public class PendingRegistrationRepository : IPendingRegistrationRepository
    {
        private readonly ApplicationDbContext _context;

        public PendingRegistrationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PendingRegistration?> GetByPhoneNumberAsync(string phoneNumber)
        {
            return await _context.PendingRegistrations.SingleOrDefaultAsync(p => p.PhoneNumber == phoneNumber);
        }

        public async Task AddAsync(PendingRegistration pendingRegistration)
        {
            await _context.PendingRegistrations.AddAsync(pendingRegistration);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PendingRegistration pendingRegistration)
        {
            _context.PendingRegistrations.Update(pendingRegistration);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var pendingRegistration = await _context.PendingRegistrations.FindAsync(id);
            if (pendingRegistration != null)
            {
                _context.PendingRegistrations.Remove(pendingRegistration);
                await _context.SaveChangesAsync();
            }
        }
    }
}
