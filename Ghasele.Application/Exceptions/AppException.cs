using Ghasele.Application.Localization;

namespace Ghasele.Application.Exceptions
{
    /// <summary>
    /// An error that is safe to show the caller. Carries an <see cref="ErrorCodes"/> value rather
    /// than prose so the Application layer stays language-agnostic; the API boundary turns the code
    /// into text using the language the client asked for.
    /// </summary>
    public class AppException : Exception
    {
        public string ErrorCode { get; }

        /// <summary>Values substituted into the message template, if it has placeholders.</summary>
        public object[] Args { get; }

        public int StatusCode { get; }

        /// <param name="errorCode">A constant from <see cref="ErrorCodes"/>.</param>
        /// <param name="statusCode">HTTP status to return. Defaults to 400.</param>
        /// <param name="args">Optional values for placeholders in the message template.</param>
        public AppException(string errorCode, int statusCode = 400, params object[] args)
            // The code doubles as the base message so logs and stack traces stay readable.
            : base(errorCode)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
            Args = args ?? Array.Empty<object>();
        }

        /// <summary>
        /// Overload that keeps the underlying failure attached, so the audit log and logger retain
        /// the technical detail while the client still only sees the localized message.
        /// </summary>
        public AppException(string errorCode, int statusCode, Exception? innerException, params object[] args)
            : base(errorCode, innerException)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
            Args = args ?? Array.Empty<object>();
        }

        /// <summary>Convenience for the very common 404 case.</summary>
        public static AppException NotFound(string errorCode, params object[] args) =>
            new(errorCode, 404, args);

        /// <summary>Convenience for 401.</summary>
        public static AppException Unauthorized(string errorCode, params object[] args) =>
            new(errorCode, 401, args);
    }
}
