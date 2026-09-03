using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;
using Ghasele.Application.Exceptions;
using Ghasele.Application.Interfaces;
using Ghasele.Application.Localization;
using Ghasele.Domain.Entities;
using Ghasele.Domain.Interfaces;

namespace Ghasele.Application.Services
{
    public class SupportTicketService : ISupportTicketService
    {
        private readonly ISupportTicketRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly IFileStorageService _fileStorage;

        // 5 MB is plenty for a phone photo and keeps a hostile upload cheap to reject.
        private const long MaxAttachmentBytes = 5 * 1024 * 1024;
        private static readonly string[] AllowedContentTypes =
            { "image/jpeg", "image/jpg", "image/png", "image/webp", "image/heic", "image/heif" };

        // Clients that build the multipart part without an explicit content type send
        // application/octet-stream, so the extension is the only signal left. App
        // versions <= 1.0.1 do exactly that, and rejecting them would break photo
        // uploads for everyone who has not updated.
        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".webp", ".heic", ".heif" };

        public SupportTicketService(ISupportTicketRepository repository, IUserRepository userRepository, IFileStorageService fileStorage)
        {
            _repository = repository;
            _userRepository = userRepository;
            _fileStorage = fileStorage;
        }

        /// <summary>
        /// Accepts the upload when either the declared content type or the file extension
        /// names an image. Both are client-supplied and trivially forged, so this is a
        /// usability guard against wrong-file uploads, not a security boundary - the
        /// stored file is never executed and is served back as a static asset.
        /// </summary>
        private static bool IsAllowedImage(TicketAttachmentUpload attachment)
        {
            var contentType = attachment.ContentType?.Trim().ToLowerInvariant();
            if (!string.IsNullOrEmpty(contentType) && Array.IndexOf(AllowedContentTypes, contentType) >= 0)
            {
                return true;
            }

            var extension = Path.GetExtension(attachment.FileName ?? string.Empty).ToLowerInvariant();
            return Array.IndexOf(AllowedExtensions, extension) >= 0;
        }

        public async Task<TicketDto> CreateTicketAsync(string userId, CreateTicketDto dto, TicketAttachmentUpload? attachment = null)
        {
            string? attachmentUrl = null;
            if (attachment != null && attachment.Length > 0)
            {
                if (attachment.Length > MaxAttachmentBytes)
                {
                    throw new AppException(ErrorCodes.TicketAttachmentTooLarge);
                }
                if (!IsAllowedImage(attachment))
                {
                    throw new AppException(ErrorCodes.TicketAttachmentInvalidType);
                }

                attachmentUrl = await _fileStorage.SaveAsync(attachment.Content, attachment.FileName, "support");
            }

            var ticket = new SupportTicket
            {
                UserId = userId,
                Subject = dto.Subject,
                Message = dto.Message,
                Category = dto.Category,
                Status = "Open",
                AttachmentUrl = attachmentUrl,
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
                AttachmentUrl = ticket.AttachmentUrl,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                UserName = user?.FullName,
                UserPhoneNumber = user?.PhoneNumber
            };
        }
    }

}
