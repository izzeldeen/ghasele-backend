using Ghasele.Application.DTOs;
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
    // [Authorize] // Enable this later after authentication is fully tested
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, UserDto userDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || user.IsDeleted) return NotFound();

            user.FullName = userDto.FullName;
            user.Username = userDto.Username;
            user.Email = userDto.Email;
            user.PhoneNumber = userDto.PhoneNumber;

            await _userRepository.UpdateAsync(user);
            return NoContent();
        }

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
