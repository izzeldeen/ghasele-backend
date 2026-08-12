using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;
using Ghasele.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

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

        [HttpPost("verify-registration-otp")]
        public async Task<IActionResult> VerifyRegistrationOtp([FromBody] VerifyRegistrationOtpRequest request)
        {
            var isValid = await _authService.VerifyRegistrationOtpAsync(request.PhoneNumber, request.Otp);
            if (isValid)
            {
                return Ok(new { success = true, message = "Phone number verified successfully." });
            }
            return BadRequest(new { success = false, message = "Invalid or expired OTP." });
        }

        [HttpPost("resend-registration-otp")]
        public async Task<IActionResult> ResendRegistrationOtp([FromBody] ResendRegistrationOtpRequest request)
        {
            try
            {
                await _authService.ResendRegistrationOtpAsync(request.PhoneNumber);
                return Ok(new { message = "OTP sent via WhatsApp successfully." });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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

        [HttpPost("apple")]
        public async Task<IActionResult> AppleSignIn([FromBody] AppleSignInRequest request)
        {
            try
            {
                var response = await _authService.AppleSignInAsync(request);
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// Required by App Store guideline 5.1.1(v) for any app offering account
        /// creation. Authorized, and scoped to the caller's own account so a
        /// valid token cannot be used to delete somebody else's.
        [Authorize]
        [HttpDelete("delete-account/{userId}")]
        public async Task<IActionResult> DeleteAccount(Guid userId)
        {
            var callerId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(callerId) || !Guid.TryParse(callerId, out var callerGuid))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            if (callerGuid != userId)
            {
                return Forbid();
            }

            try
            {
                await _authService.DeleteAccountAsync(userId);
                return NoContent();
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

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                await _authService.ForgotPasswordAsync(request.PhoneNumber);
                return Ok(new { message = "OTP sent via WhatsApp successfully." });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var isValid = await _authService.VerifyResetPasswordOtpAsync(request.PhoneNumber, request.Otp);
            if (isValid)
            {
                return Ok(new { success = true, message = "OTP verified successfully." });
            }
            return BadRequest(new { success = false, message = "Invalid or expired OTP." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                await _authService.ResetPasswordAsync(request.PhoneNumber, request.Otp, request.NewPassword);
                return Ok(new { success = true, message = "Password reset successfully." });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
