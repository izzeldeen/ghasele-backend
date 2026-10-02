using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Ghasele.API.Controllers
{
    /// <summary>
    /// Forced-update policy for the customer app. The app asks on every launch and blocks
    /// itself behind an "update now" screen when its build number is below the minimum.
    /// </summary>
    /// <remarks>
    /// Values live in the "AppUpdate" section of appsettings.json, per platform, so a release
    /// is enforced by raising MinimumBuild there and restarting - no code change. Raise it only
    /// once that build is actually live in the store, or users are sent to update to a version
    /// they cannot download yet. A MinimumBuild of 0 turns enforcement off for that platform.
    /// <para>
    /// Anonymous on purpose: the check runs on the splash screen, before anyone signs in.
    /// </para>
    /// </remarks>
    [ApiController]
    [Route("api/app-version")]
    [AllowAnonymous]
    public class AppVersionController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AppVersionController(IConfiguration config)
        {
            _config = config;
        }

        /// <param name="platform">"ios" or "android".</param>
        [HttpGet]
        public IActionResult Get([FromQuery] string platform)
        {
            var key = platform?.Trim().ToLowerInvariant() switch
            {
                "ios" => "iOS",
                "android" => "Android",
                _ => null
            };
            if (key == null)
            {
                return BadRequest(new { message = "platform must be 'ios' or 'android'." });
            }

            var section = _config.GetSection($"AppUpdate:{key}");
            return Ok(new
            {
                minimumBuild = section.GetValue<int>("MinimumBuild"),
                storeUrl = section["StoreUrl"] ?? string.Empty
            });
        }
    }
}
