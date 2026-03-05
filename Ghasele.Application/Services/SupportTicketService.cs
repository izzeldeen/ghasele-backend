using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;
using Ghasele.Domain.Entities;
using Ghasele.Domain.Interfaces;

namespace Ghasele.Application.Services
{
    public class SupportTicketService : ISupportTicketService
    {
        private readonly ISupportTicketRepository _repository;
        private readonly IUserRepository _userRepository;

        public SupportTicketService(ISupportTicketRepository repository, IUserRepository userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
        }

        public async Task<TicketDto> CreateTicketAsync(string userId, CreateTicketDto dto)
        {
            var ticket = new SupportTicket
            {
                UserId = userId,
                Subject = dto.Subject,
                Message = dto.Message,
                Category = dto.Category,
                Status = "Open",
                CreatedAt = DateTime.UtcNow
            };

            var createdTicket = await _repository.CreateAsync(ticket);
            var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
            return MapToDto(createdTicket, user);
        }

        public async Task<IEnumerable<TicketDto>> GetUserTicketsAsync(string userId)
        {
            var tickets = await _repository.GetByUserIdAsync(userId);
            var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
            return tickets.Select(t => MapToDto(t, user));
        }

        public async Task<TicketDto?> GetTicketByIdAsync(int id)
        {
            var ticket = await _repository.GetByIdAsync(id);
            if (ticket == null) return null;
            
            var user = await _userRepository.GetByIdAsync(Guid.Parse(ticket.UserId));
            return MapToDto(ticket, user);
        }

        public async Task<TicketDto?> RespondToTicketAsync(int id, string response)
        {
            var ticket = await _repository.GetByIdAsync(id);
            if (ticket == null) return null;

            ticket.Response = response;
            ticket.Status = "Resolved";
            ticket.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(ticket);
            
            var user = await _userRepository.GetByIdAsync(Guid.Parse(ticket.UserId));
            return MapToDto(ticket, user);
        }

        public async Task<IEnumerable<TicketDto>> GetAllTicketsAsync()
        {
            var tickets = await _repository.GetAllAsync();
            var users = await _userRepository.GetAllAsync();
            var userDict = users.ToDictionary(u => u.Id.ToString(), u => u);

            return tickets.Select(t => 
            {
                userDict.TryGetValue(t.UserId, out var user);
                return MapToDto(t, user);
            });
        }

        private static TicketDto MapToDto(SupportTicket ticket, User? user)
        {
            return new TicketDto
            {
                Id = ticket.Id,
                UserId = ticket.UserId,
                Subject = ticket.Subject,
                Message = ticket.Message,
                Category = ticket.Category,
                Status = ticket.Status,
                Response = ticket.Response,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                UserName = user?.FullName,
                UserPhoneNumber = user?.PhoneNumber
            };
        }
    }

}
