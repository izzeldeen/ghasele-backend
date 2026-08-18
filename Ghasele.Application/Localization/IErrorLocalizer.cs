namespace Ghasele.Application.Localization
{
    /// <summary>
    /// Resolves an <see cref="ErrorCodes"/> value into human-readable text for a given language.
    /// Implementations must never throw for an unknown code or language; they fall back instead.
    /// </summary>
    public interface IErrorLocalizer
    {
        /// <summary>Default language used when the request specifies none we support.</summary>
        string DefaultLanguage { get; }

        /// <summary>Languages this localizer has a catalog for (two-letter ISO codes).</summary>
        IReadOnlyCollection<string> SupportedLanguages { get; }

        /// <summary>True when <paramref name="language"/> has a catalog.</summary>
        bool IsSupported(string? language);

        /// <summary>
        /// Returns the message for <paramref name="code"/> in <paramref name="language"/>, falling
        /// back to <see cref="DefaultLanguage"/> and finally to the raw code.
        /// </summary>
        string Localize(string code, string? language, params object[] args);
    }
}
