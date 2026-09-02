using System.Threading.Tasks;
using Ghasele.Application.DTOs;
using Ghasele.Application.Interfaces;
using Ghasele.Application.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Ghasele.API.Controllers
{
    /// <summary>
    /// Firebase Phone Authentication sign-in.
    /// </summary>
    /// <remarks>
    /// Kept as its own controller, sharing the <c>api/auth</c> prefix, so the Firebase path can be
    /// added, disabled or removed without touching <see cref="AuthController"/> and the WhatsApp OTP
    /// endpoints it owns. Both remain live: clients may use either.
    /// </remarks>
    [ApiController]
    [Route("api/auth")]
    public class FirebaseAuthController : ApiControllerBase
    {
        private readonly IAuthService _authService;

        public FirebaseAuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Exchanges a Firebase ID token for one of our own JWTs, creating the account on first
        /// sign-in. Anonymous by design - the Firebase token is the credential being presented.
        /// </summary>
        /// <response code="200">Verified. Returns our JWT and the user profile.</response>
        /// <response code="400">No ID token in the body.</response>
        /// <response code="401">Token expired, revoked, malformed, or carrying no phone number.</response>
        [HttpPost("firebase-login")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> FirebaseLogin([FromBody] FirebaseLoginRequest request)
        {
            // Model binding yields null for an empty or malformed body; answer 400 here rather than
            // dereferencing it. Every other failure is raised as an AppException and turned into the
            // right status and language by ExceptionHandlingMiddleware, so there is no try/catch -
            // catching here would emit the raw error code instead of localized text.
            if (request is null || string.IsNullOrWhiteSpace(request.IdToken))
            {
                return BadRequest(new
                {
                    errorCode = ErrorCodes.FirebaseTokenMissing,
                    message = L(ErrorCodes.FirebaseTokenMissing)
                });
            }

            var response = await _authService.FirebaseLoginAsync(request);
            return Ok(response);
        }
    }
}
