using System.Threading.Tasks;
using Ghasele.Application.DTOs;
using Ghasele.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ghasele.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] RegisterRequest request)
        {
            System.Console.WriteLine($"[BACKEND] SignUp Request: Phone={request.PhoneNumber}, Name={request.FullName}");
            var response = await _authService.RegisterAsync(request);
            return Ok(response);
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] LoginRequest request)
        {
            System.Console.WriteLine($"[BACKEND] SignIn Request: Phone={request.PhoneNumber}");
            try
            {
                var response = await _authService.LoginAsync(request);
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update-fcm-token")]
        public async Task<IActionResult> UpdateFcmToken([FromBody] UpdateFcmTokenRequest request)
        {
            try
            {
                await _authService.UpdateFcmTokenAsync(request.UserId, request.Token);
                return Ok(new { message = "Token updated successfully" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
