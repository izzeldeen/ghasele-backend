namespace Ghasele.API.Localization
{
    /// <summary>
    /// Localizes messages into the language of the request currently being handled.
    /// </summary>
    public interface IRequestLocalizer
    {
        /// <summary>The two-letter language resolved for this request (e.g. "en", "ar").</summary>
        string Language { get; }

        /// <summary>Localizes an <c>ErrorCodes</c> value into the request's language.</summary>
        string L(string code, params object[] args);
    }
}
