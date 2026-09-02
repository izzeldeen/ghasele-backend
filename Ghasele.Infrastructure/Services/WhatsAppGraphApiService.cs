using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Ghasele.Application.Exceptions;
using Ghasele.Application.Interfaces;
using Ghasele.Application.Localization;
using Microsoft.Extensions.Configuration;

namespace Ghasele.Infrastructure.Services
{
    /// <summary>
    /// Sends WhatsApp messages by calling Meta's Graph API directly (no third-party SDK), so
    /// failures surface Meta's actual error body instead of an opaque wrapper exception.
    /// </summary>
    /// <remarks>
    /// The OTP is delivered through the pre-approved <c>verify_code_1</c> template (one body
    /// parameter, <c>{{code}}</c>). A template is mandatory here because the recipient has not
    /// necessarily messaged the business number in the last 24 hours, which is the only case
    /// where Meta allows a free-form text message.
    /// </remarks>
    public class WhatsAppGraphApiService : IWhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _phoneNumberId;
        private readonly string? _accessToken;
        private readonly string _apiVersion;
        private readonly string _templateName;
        private readonly string _templateLanguage;

        public WhatsAppGraphApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _phoneNumberId = configuration["WhatsApp:PhoneNumberId"];
            _accessToken = configuration["WhatsApp:AccessToken"];
            _apiVersion = configuration["WhatsApp:ApiVersion"] ?? "v20.0";
            _templateName = configuration["WhatsApp:VerifyTemplateName"] ?? "verify_code_1";
            _templateLanguage = configuration["WhatsApp:VerifyTemplateLanguage"] ?? "en";
        }

        public async Task SendOtpAsync(string phoneNumber, string code)
        {
            if (string.IsNullOrWhiteSpace(_phoneNumberId) || string.IsNullOrWhiteSpace(_accessToken))
            {
                throw new AppException(ErrorCodes.WhatsAppSendFailed, 500, "WhatsApp:PhoneNumberId/AccessToken are not configured on the server.");
            }

            var url = $"https://graph.facebook.com/{_apiVersion}/{_phoneNumberId}/messages";

            var payload = new WhatsAppTemplateMessageRequest(
                MessagingProduct: "whatsapp",
                To: phoneNumber.TrimStart('+'),
                Type: "template",
                Template: new WhatsAppTemplate(
                    Name: _templateName,
                    Language: new WhatsAppLanguage(_templateLanguage),
                    Components: new[]
                    {
                        new WhatsAppComponent(
                            Type: "body",
                            Parameters: new[] { new WhatsAppParameter("text", code) })
                    }));

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(payload, options: JsonOpts)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new AppException(ErrorCodes.WhatsAppSendFailed, (int)response.StatusCode, ExtractMetaErrorMessage(body));
            }
        }

        // Drop null properties so a body-only template request never carries an empty components field.
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>Meta's error shape: <c>{ "error": { "message", "type", "code", "fbtrace_id" } }</c>.</summary>
        private static string ExtractMetaErrorMessage(string responseBody)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseBody);
                if (doc.RootElement.TryGetProperty("error", out var error) &&
                    error.TryGetProperty("message", out var message))
                {
                    return message.GetString() ?? responseBody;
                }
            }
            catch (JsonException)
            {
                // Fall through and surface the raw body below.
            }

            return responseBody;
        }

        private record WhatsAppTemplateMessageRequest(
            [property: JsonPropertyName("messaging_product")] string MessagingProduct,
            [property: JsonPropertyName("to")] string To,
            [property: JsonPropertyName("type")] string Type,
            [property: JsonPropertyName("template")] WhatsAppTemplate Template);

        private record WhatsAppTemplate(
            [property: JsonPropertyName("name")] string Name,
            [property: JsonPropertyName("language")] WhatsAppLanguage Language,
            [property: JsonPropertyName("components")] WhatsAppComponent[]? Components);

        private record WhatsAppLanguage(
            [property: JsonPropertyName("code")] string Code);

        private record WhatsAppComponent(
            [property: JsonPropertyName("type")] string Type,
            [property: JsonPropertyName("parameters")] WhatsAppParameter[] Parameters);

        private record WhatsAppParameter(
            [property: JsonPropertyName("type")] string Type,
            [property: JsonPropertyName("text")] string Text);
    }
}
