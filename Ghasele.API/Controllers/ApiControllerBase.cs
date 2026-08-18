using Ghasele.API.Localization;
using Ghasele.Application.Exceptions;
using Ghasele.Application.Localization;
using Microsoft.AspNetCore.Mvc;

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
