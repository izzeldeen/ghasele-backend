namespace Ghasele.Application.Localization
{
    /// <summary>
    /// Picks the display string for an entity that stores its name in both Arabic and English
    /// (drivers, cleaners, item types) rather than a single language-neutral value.
    /// </summary>
    public static class BilingualText
    {
        /// <summary>
        /// Returns <paramref name="ar"/> or <paramref name="en"/> for <paramref name="language"/>,
        /// falling back to whichever of the two is non-empty if the preferred one was never filled in.
        /// </summary>
        public static string Pick(string? ar, string? en, string language)
        {
            var preferred = language == "ar" ? ar : en;
            var fallback = language == "ar" ? en : ar;
            return !string.IsNullOrWhiteSpace(preferred) ? preferred! : (fallback ?? string.Empty);
        }
    }
}
