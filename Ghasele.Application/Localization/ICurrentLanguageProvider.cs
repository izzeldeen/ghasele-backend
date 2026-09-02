namespace Ghasele.Application.Localization
{
    /// <summary>
    /// Resolves the language of the request currently being handled, for services that pick a
    /// localized value (e.g. a bilingual name) rather than translate an error message.
    /// </summary>
    public interface ICurrentLanguageProvider
    {
        /// <summary>The two-letter language resolved for this request (e.g. "en", "ar").</summary>
        string Language { get; }
    }
}
