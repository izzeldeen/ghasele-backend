using Ghasele.Application.Localization;
using Microsoft.Net.Http.Headers;

namespace Ghasele.API.Localization
{
    /// <summary>
    /// Resolves the caller's language from the request and localizes messages with it.
    /// </summary>
    /// <remarks>
    /// Language is taken from the standard <c>Accept-Language</c> header, with a non-standard
    /// <c>X-Language</c> header as an explicit override. The header is used rather than a stored
    /// user preference because the errors users hit most — bad password, unregistered number,
    /// expired OTP — all occur on anonymous requests where there is no user to read a preference
    /// from.
    /// </remarks>
    public class RequestLocalizer : IRequestLocalizer
    {
        private const string OverrideHeader = "X-Language";
        private const string CacheKey = "__resolved_language";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IErrorLocalizer _localizer;

        public RequestLocalizer(IHttpContextAccessor httpContextAccessor, IErrorLocalizer localizer)
        {
            _httpContextAccessor = httpContextAccessor;
            _localizer = localizer;
        }

        public string Language
        {
            get
            {
                var context = _httpContextAccessor.HttpContext;
                if (context is null)
                {
                    return _localizer.DefaultLanguage;
                }

                // Accept-Language parsing runs on every localized message, so memoize per request.
                if (context.Items.TryGetValue(CacheKey, out var cached) && cached is string cachedLanguage)
                {
                    return cachedLanguage;
                }

                var resolved = Resolve(context);
                context.Items[CacheKey] = resolved;
                return resolved;
            }
        }

        public string L(string code, params object[] args) => _localizer.Localize(code, Language, args);

        private string Resolve(HttpContext context)
        {
            var overridden = context.Request.Headers[OverrideHeader].FirstOrDefault();
            if (_localizer.IsSupported(overridden))
            {
                return overridden!;
            }

            var accepted = context.Request.Headers.AcceptLanguage
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value!)
                .ToArray();

            if (accepted.Length > 0 &&
                StringWithQualityHeaderValue.TryParseList(accepted, out var parsed) &&
                parsed is not null)
            {
                // Honour the client's stated preference order rather than taking the first entry.
                foreach (var entry in parsed.OrderByDescending(v => v.Quality ?? 1.0))
                {
                    var value = entry.Value.ToString();
                    if (value == "*")
                    {
                        continue;
                    }

                    if (_localizer.IsSupported(value))
                    {
                        return value;
                    }
                }
            }

            return _localizer.DefaultLanguage;
        }
    }
}
