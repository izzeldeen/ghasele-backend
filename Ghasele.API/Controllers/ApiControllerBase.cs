using System;
using System.Threading.Tasks;
using Ghasele.API.Localization;
using Ghasele.Application.DTOs;
using Ghasele.Application.Exceptions;
using Ghasele.Application.Interfaces;
using Ghasele.Application.Localization;
using Ghasele.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ghasele.API.Controllers
{
    /// <summary>
    /// Base for controllers that return user-facing text. Exposes <see cref="L"/> for localizing
    /// <c>ErrorCodes</c> values into the caller's language.
    /// </summary>
    /// <remarks>
    /// The localizer is resolved lazily from the request services rather than injected, so adopting
    /// this base class does not require touching each controller's constructor.
    /// </remarks>
    public abstract class ApiControllerBase : ControllerBase
    {
        private IRequestLocalizer? _localizer;

        protected IRequestLocalizer Localizer =>
            _localizer ??= HttpContext.RequestServices.GetRequiredService<IRequestLocalizer>();

        /// <summary>Localizes an <c>ErrorCodes</c> value into the current request's language.</summary>
        protected string L(string code, params object[] args) => Localizer.L(code, args);

        /// <summary>
        /// The per-install identifier of the calling device, or null when the caller sent none.
        /// </summary>
        /// <remarks>
        /// Read from the <c>X-Device-Token</c> header rather than a query string or body, so it
        /// stays out of access logs and cannot be set to someone else's value by an anonymous
        /// caller. It is what ties a guest's orders and tickets to the app that placed them, and
        /// what lets those records be claimed once that person signs in - so every controller that
        /// needs it must read it the same way.
        /// </remarks>
        protected string? DeviceToken()
        {
            var value = Request.Headers["X-Device-Token"].ToString();
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        /// <summary>
        /// Hands the orders placed as a guest on this device to the account that has just signed
        /// in, so ordering before registering does not cost the customer their history.
        /// </summary>
        /// <remarks>
        /// Call this on every path that hands back a token - password sign-in, Apple, Google,
        /// Firebase phone, and the two registration completions - because a guest can arrive at an
        /// account through any of them.
        /// <para>
        /// Only for customers. A captain or an admin signing in on a handset that was once used to
        /// order as a guest must not inherit those orders, and role is the only thing separating
        /// the two cases.
        /// </para>
        /// <para>
        /// Failures are swallowed on purpose. The customer has been authenticated by the time this
        /// runs, and refusing them the token they just earned because a follow-up update failed
        /// would turn a missing link into a failed sign-in. The orders stay claimable: they keep
        /// their device token, so the next sign-in on this device picks them up.
        /// </para>
        /// </remarks>
        protected async Task ClaimGuestOrdersAsync(AuthResponse auth)
        {
            if (auth is null || !string.Equals(auth.Role, nameof(UserRole.Client), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var deviceToken = DeviceToken();
            if (string.IsNullOrWhiteSpace(deviceToken))
            {
                return;
            }

            try
            {
                var orders = HttpContext.RequestServices.GetRequiredService<IOrderService>();
                await orders.ClaimGuestOrdersAsync(auth.Id, deviceToken);
            }
            catch (Exception ex)
            {
                HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger(GetType())
                    .LogError(ex, "Failed to claim guest orders for user {UserId}", auth.Id);
            }
        }

        /// <summary>
        /// Builds the error body for a caught exception. An <see cref="AppException"/> yields its
        /// own localized message; anything else is reported generically, because its text is
        /// internal detail that must not reach the caller.
        /// </summary>
        protected object ErrorBody(Exception ex)
        {
            if (ex is AppException appException)
            {
                return new
                {
                    errorCode = appException.ErrorCode,
                    message = L(appException.ErrorCode, appException.Args)
                };
            }

            return new
            {
                errorCode = ErrorCodes.InternalError,
                message = L(ErrorCodes.InternalError)
            };
        }
    }
}
