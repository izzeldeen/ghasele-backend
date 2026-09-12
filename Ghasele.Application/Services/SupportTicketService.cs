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

        public async Task<TicketDto> CreateTicketAsync(string? userId, string? deviceToken, CreateTicketDto dto, TicketAttachmentUpload? attachment = null)
        {
            var isGuest = string.IsNullOrWhiteSpace(userId);
            var contactPhoneNumber = dto.ContactPhoneNumber?.Trim();
            deviceToken = deviceToken?.Trim();

            if (isGuest)
            {
                // A guest ticket has no user row behind it, so these two are the only ways to
                // answer the customer: the number to reach them, and the device token that lets
                // the app show them the reply.
                if (string.IsNullOrWhiteSpace(contactPhoneNumber))
                {
                    throw new AppException(ErrorCodes.TicketContactNumberRequired);
                }
                if (string.IsNullOrWhiteSpace(deviceToken))
                {
                    throw new AppException(ErrorCodes.GuestDeviceTokenRequired);
                }
            }

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
                UserId = isGuest ? null : userId,
                // Guest-only, mirroring orders: a signed-in customer is found by their account,
                // and storing a device against them would tie their tickets to one phone.
                DeviceToken = isGuest ? deviceToken : null,
                ContactPhoneNumber = isGuest ? contactPhoneNumber : null,
                Subject = dto.Subject,
                Message = dto.Message,
                Category = dto.Category,
                Status = "Open",
                AttachmentUrl = attachmentUrl,
                CreatedAt = DateTime.UtcNow
            };

            var createdTicket = await _repository.CreateAsync(ticket);
            return MapToDto(createdTicket, await LoadUserAsync(ticket.UserId));
        }

        public async Task<IEnumerable<TicketDto>> GetUserTicketsAsync(string userId)
        {
            var tickets = await _repository.GetByUserIdAsync(userId);
            var user = await LoadUserAsync(userId);
            return tickets.Select(t => MapToDto(t, user));
        }

        public async Task<IEnumerable<TicketDto>> GetGuestTicketsAsync(string deviceToken)
        {
            var tickets = await _repository.GetByDeviceTokenAsync(deviceToken);
            // No user to join to - a guest ticket carries its own contact number instead.
            return tickets.Select(t => MapToDto(t, null));
        }

        /// <summary>
        /// Loads the ticket's owner, or null for a guest ticket. Ids that are not parseable as a
        /// Guid are treated as absent rather than throwing: the column is free text, and one bad
        /// row must not take down the whole listing.
        /// </summary>
        private async Task<User?> LoadUserAsync(string? userId)
        {
            return Guid.TryParse(userId, out var id) ? await _userRepository.GetByIdAsync(id) : null;
        }

        public async Task<TicketDto?> GetTicketByIdAsync(int id)
        {
            var ticket = await _repository.GetByIdAsync(id);
            if (ticket == null) return null;
            
            var user = await LoadUserAsync(ticket.UserId);
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
            
            var user = await LoadUserAsync(ticket.UserId);
            return MapToDto(ticket, user);
        }

        public async Task<IEnumerable<TicketDto>> GetAllTicketsAsync()
        {
            var tickets = await _repository.GetAllAsync();
            var users = await _userRepository.GetAllAsync();
            var userDict = users.ToDictionary(u => u.Id.ToString(), u => u);

            return tickets.Select(t => 
            {
                // Guest tickets have no owner, and a null key would throw here.
                User? user = null;
                if (t.UserId != null) userDict.TryGetValue(t.UserId, out user);
                return MapToDto(t, user);
            });
        }

        private static TicketDto MapToDto(SupportTicket ticket, User? user)
        {
            return new TicketDto
            {
                Id = ticket.Id,
                UserId = ticket.UserId ?? string.Empty,
                IsGuest = ticket.UserId == null,
                Subject = ticket.Subject,
                Message = ticket.Message,
                Category = ticket.Category,
                Status = ticket.Status,
                Response = ticket.Response,
                AttachmentUrl = ticket.AttachmentUrl,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                UserName = user?.FullName,
                // Falls back to the number the guest typed, so the admin screen has someone to
                // call either way rather than an empty column on half the tickets.
                UserPhoneNumber = user?.PhoneNumber ?? ticket.ContactPhoneNumber
            };
        }
    }

}
