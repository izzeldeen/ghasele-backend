using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Ghasele.API.Controllers
{
    /// <summary>
    /// Throwaway diagnostic endpoint for validating Meta WhatsApp Cloud API sandbox credentials
    /// directly against the Graph API, independent of the app's real <c>IWhatsAppService</c> flow.
    /// Delete this controller (or lock it behind auth/remove the hardcoded token) once the sandbox
    /// credentials are confirmed working - the access token below is a real secret, not a public
    /// key, so it must never be committed to source control.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WhatsAppTestController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        // TODO: replace with your sandbox values from Meta for Developers > WhatsApp > API Setup.
        // These expire quickly (temporary tokens last ~24h) - regenerate as needed while testing.
        private const string PhoneNumberId = "1304141699448438";
        private const string AccessToken = "EAAUQS7o0pKcBSY0swCqFcZBw0dmkZCCfYnEkKnMUSkqptI4GhINPrJkzZCPAPgkW2TMnifk1hSojXBZCV6vws9Uy7ZAkanyjjLC6BS7B4HUzp98PR0WgnRkplvqGUwrifLA2yUmudMkw8loO37Aq1eYZCUWMYKHYxagYbqQJZAFpp0jWMKv7AdANLfFbrgwCeYZC3arOQUoinmZA1YbbhBmvzCWmG8yNwc4WELka3IYTu3y84VV1UYRrKS2Ly2wLFiQoSVhSkZAYvDBNr1ZBS2tZAIfFhlg2zZBWXTienJST1WwZDZD";
        private const string TestRecipientNumber = "962797237416"; // E.164 without the leading '+'

        // Meta treats every (template name, language) pair as a distinct template. #132001 means
        // the name exists but not under the language you passed - try each of these to find it.
        private static readonly string[] CandidateLanguages = { "en", "en_US", "ar" };

        public WhatsAppTestController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Sends the <c>verify_code_1</c> template with its single {{code}} body parameter.
        /// <c>POST /api/whatsapptest/send-test-whatsapp?lang=en&amp;code=123456&amp;to=9627XXXXXXXX</c>
        /// All query params optional - <c>lang</c> defaults to trying en, en_US, ar in order and
        /// returns the first that Meta accepts.
        /// </summary>
        [HttpPost("send-test-whatsapp")]
        public async Task<IActionResult> SendTestWhatsApp(
            [FromQuery] string? lang,
            [FromQuery] string? code,
            [FromQuery] string? to)
        {
            var recipient = (to ?? TestRecipientNumber).TrimStart('+');
            var otp = string.IsNullOrWhiteSpace(code) ? "123456" : code;
            var languagesToTry = string.IsNullOrWhiteSpace(lang) ? CandidateLanguages : new[] { lang };

            var attempts = new List<object>();

            foreach (var language in languagesToTry)
            {
                var (status, body) = await SendTemplateAsync(recipient, language, otp);
                attempts.Add(new { language, status, response = TryParseJson(body) ?? (object)body });

                if (status >= 200 && status < 300)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"Template accepted with language '{language}'. Put this in WhatsApp:VerifyTemplateLanguage.",
                        language,
                        attempts
                    });
                }
            }

            return StatusCode(502, new
            {
                success = false,
                message = "No candidate language was accepted. Check the exact language in WhatsApp Manager > Message Templates.",
                attempts
            });
        }

        private async Task<(int Status, string Body)> SendTemplateAsync(string recipient, string language, string code)
        {
            var url = $"https://graph.facebook.com/v20.0/{PhoneNumberId}/messages";

            var payload = new WhatsAppTemplateMessageRequest(
                MessagingProduct: "whatsapp",
                To: recipient,
                Type: "template",
                Template: new WhatsAppTemplate(
                    "verify_code_1",
                    new WhatsAppLanguage(language),
                    new[]
                    {
                        new WhatsAppComponent("body", new[] { new WhatsAppParameter("text", code) })
                    }));

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(payload, options: new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            using var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            return ((int)response.StatusCode, body);
        }

        private static object? TryParseJson(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<JsonElement>(json);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }

    public record WhatsAppTemplateMessageRequest(
        [property: JsonPropertyName("messaging_product")] string MessagingProduct,
        [property: JsonPropertyName("to")] string To,
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("template")] WhatsAppTemplate Template);

    public record WhatsAppTemplate(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("language")] WhatsAppLanguage Language,
        [property: JsonPropertyName("components")] WhatsAppComponent[]? Components);

    public record WhatsAppLanguage(
        [property: JsonPropertyName("code")] string Code);

    public record WhatsAppComponent(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("parameters")] WhatsAppParameter[] Parameters);

    public record WhatsAppParameter(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("text")] string Text);
}
