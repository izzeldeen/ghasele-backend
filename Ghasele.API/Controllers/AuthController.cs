using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;
using Ghasele.Application.Interfaces;
using Ghasele.Application.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Ghasele.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ApiControllerBase
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
                return Ok(new { success = true, message = L(ErrorCodes.PhoneVerified) });
            }
            return BadRequest(new { success = false, errorCode = ErrorCodes.OtpInvalidOrExpired, message = L(ErrorCodes.OtpInvalidOrExpired) });
        }

        [HttpPost("resend-registration-otp")]
        public async Task<IActionResult> ResendRegistrationOtp([FromBody] ResendRegistrationOtpRequest request)
        {
            // Failures propagate to ExceptionHandlingMiddleware, which localizes them into the
            // caller's language. Catching here would surface the raw error code instead.
            await _authService.ResendRegistrationOtpAsync(request.PhoneNumber);
            return Ok(new { message = L(ErrorCodes.OtpSent) });
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] LoginRequest request)
        {
            System.Console.WriteLine($"[BACKEND] SignIn Request: Phone={request.PhoneNumber}");
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }

        [HttpPost("apple")]
        public async Task<IActionResult> AppleSignIn([FromBody] AppleSignInRequest request)
        {
            var response = await _authService.AppleSignInAsync(request);
            return Ok(response);
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
                return Unauthorized(new { errorCode = ErrorCodes.InvalidToken, message = L(ErrorCodes.InvalidToken) });
            }

            if (callerGuid != userId)
            {
                return Forbid();
            }

            await _authService.DeleteAccountAsync(userId);
            return NoContent();
        }

        [HttpPut("update-fcm-token")]
        public async Task<IActionResult> UpdateFcmToken([FromBody] UpdateFcmTokenRequest request)
        {
            await _authService.UpdateFcmTokenAsync(request.UserId, request.Token);
            return Ok(new { message = L(ErrorCodes.FcmTokenUpdated) });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            await _authService.ForgotPasswordAsync(request.PhoneNumber);
            return Ok(new { message = L(ErrorCodes.OtpSent) });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var isValid = await _authService.VerifyResetPasswordOtpAsync(request.PhoneNumber, request.Otp);
            if (isValid)
            {
                return Ok(new { success = true, message = L(ErrorCodes.OtpVerified) });
            }
            return BadRequest(new { success = false, errorCode = ErrorCodes.OtpInvalidOrExpired, message = L(ErrorCodes.OtpInvalidOrExpired) });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            await _authService.ResetPasswordAsync(request.PhoneNumber, request.Otp, request.NewPassword);
            return Ok(new { success = true, message = L(ErrorCodes.PasswordReset) });
        }
    }
}
