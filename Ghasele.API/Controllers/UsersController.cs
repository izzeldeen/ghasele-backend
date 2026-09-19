using Ghasele.Application.DTOs;
using Ghasele.Application.Localization;
using Ghasele.Domain.Entities;
using Ghasele.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ghasele.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Every action here reads or writes somebody's account, so the whole controller is
    // authorized and each action is scoped below: the listing and the delete are the
    // dashboard's, while the update is the one endpoint a customer calls on themselves.
    [Authorize]
    public class UsersController : ApiControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>Every customer's name, email and phone number - the dashboard's user list.</summary>
        /// <remarks>Admin only: this is the whole customer database in one response.</remarks>
        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userRepository.GetAllAsync();
            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FullName = u.FullName,
                PhoneNumber = u.PhoneNumber
            });

            return Ok(userDtos);
        }

        /// <summary>Edits an account's name, username, email and phone number.</summary>
        /// <remarks>
        /// Scoped to the caller's own account unless they are an admin. Without that check a
        /// valid token for any account could rewrite any other - and because the phone number
        /// is what the OTP sign-in looks an account up by, rewriting someone else's would hand
        /// their account over rather than merely vandalise it.
        /// </remarks>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, UserDto userDto)
        {
            if (!IsSelfOrAdmin(id)) return Forbid();

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || user.IsDeleted) return NotFound();

            // A phone number identifies an account at sign-in, so two rows may not share one.
            // Checked only when it actually changes, so saving an unrelated field on an account
            // whose number is already stored cannot fail against itself.
            if (!string.IsNullOrWhiteSpace(userDto.PhoneNumber) &&
                !string.Equals(userDto.PhoneNumber, user.PhoneNumber, StringComparison.Ordinal))
            {
                var existing = await _userRepository.GetByPhoneNumberAsync(userDto.PhoneNumber);
                if (existing != null && existing.Id != id && !existing.IsDeleted)
                {
                    return Conflict(new
                    {
                        errorCode = ErrorCodes.PhoneAlreadyExists,
                        message = L(ErrorCodes.PhoneAlreadyExists)
                    });
                }
            }

            user.FullName = userDto.FullName;
            user.Username = userDto.Username;
            user.Email = userDto.Email;
            user.PhoneNumber = userDto.PhoneNumber;

            await _userRepository.UpdateAsync(user);
            return NoContent();
        }

        /// <summary>The dashboard's delete. A customer deleting their own account goes through
        /// <c>DELETE /api/auth/delete-account/{userId}</c>, which also clears their data.</summary>
        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || user.IsDeleted) return NotFound();

            await _userRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}
