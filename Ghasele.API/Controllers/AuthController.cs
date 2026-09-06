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

        // Step 1: user enters a phone number, we send a WhatsApp OTP.
        [HttpPost("start-registration")]
        public async Task<IActionResult> StartRegistration([FromBody] StartRegistrationRequest request)
        {
            System.Console.WriteLine($"[BACKEND] StartRegistration Request: Phone={request.PhoneNumber}");
            var response = await _authService.StartRegistrationAsync(request.PhoneNumber);
            return Ok(response);
        }

        /// <summary>
        /// Whether a phone number already has an account, asked before step 1 dispatches a code.
        /// </summary>
        /// <remarks>
        /// The WhatsApp path is refused by <c>start-registration</c> itself, but the Firebase path
        /// never touches this API until the code has already been sent and the user has filled in a
        /// name and password - so without this the duplicate only surfaced at the very last step.
        /// <para>
        /// Anonymous by necessity: it is consulted before any account or token exists. It does
        /// confirm whether a number is registered, which the 409 from <c>start-registration</c>
        /// already revealed, so it opens no path that was previously closed.
        /// </para>
        /// </remarks>
        /// <response code="200">Always. The answer is in the body, not the status code.</response>
        [HttpPost("phone-registered")]
        [ProducesResponseType(typeof(PhoneRegisteredResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> PhoneRegistered([FromBody] PhoneRegisteredRequest request)
        {
            // A missing or blank number is answered "not registered" rather than 400: the client
            // validates the format first, and a 400 here would block signup on a bad round trip.
            var registered = request is not null
                && await _authService.IsPhoneRegisteredAsync(request.PhoneNumber ?? string.Empty);

            return Ok(new PhoneRegisteredResponse(
                registered,
                registered ? L(ErrorCodes.PhoneAlreadyExists) : null));
        }

        // Step 2: user enters the code. On success the phone is marked verified but no account
        // exists yet - the client moves on to collect a name and password.
        [HttpPost("verify-registration-otp")]
        public async Task<IActionResult> VerifyRegistrationOtp([FromBody] VerifyRegistrationOtpRequest request)
        {
            // Failures propagate to ExceptionHandlingMiddleware, which localizes them into the
            // caller's language. Catching here would surface the raw error code instead.
            var response = await _authService.VerifyRegistrationOtpAsync(request.PhoneNumber, request.Otp);
            return Ok(new { verified = true, phoneNumber = response.PhoneNumber, message = response.Message });
        }

        [HttpPost("resend-registration-otp")]
        public async Task<IActionResult> ResendRegistrationOtp([FromBody] ResendRegistrationOtpRequest request)
        {
            // Failures propagate to ExceptionHandlingMiddleware, which localizes them into the
            // caller's language. Catching here would surface the raw error code instead.
            await _authService.ResendRegistrationOtpAsync(request.PhoneNumber);
            return Ok(new { message = L(ErrorCodes.OtpSent) });
        }

        // Step 3: phone is verified, so create the account from the name + password and return a
        // token so the client is logged straight in.
        [HttpPost("complete-registration")]
        public async Task<IActionResult> CompleteRegistration([FromBody] CompleteRegistrationRequest request)
        {
            System.Console.WriteLine($"[BACKEND] CompleteRegistration Request: Phone={request.PhoneNumber}, Name={request.FullName}");
            var response = await _authService.CompleteRegistrationAsync(request);
            return Ok(response);
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
