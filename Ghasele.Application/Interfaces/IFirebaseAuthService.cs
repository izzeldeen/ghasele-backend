using System.Threading.Tasks;

namespace Ghasele.Application.Interfaces
{
    /// <summary>
    /// Verifies Firebase ID tokens produced by client-side Firebase Phone Authentication.
    /// </summary>
    /// <remarks>
    /// Deliberately narrow: this type answers only "is this token genuine, and whose verified phone
    /// number does it carry?". Deciding what to do with that number - find the user, create one,
    /// issue a JWT - is account policy and lives in <see cref="IAuthService"/>, so this stays a thin,
    /// testable seam over the Firebase SDK.
    /// </remarks>
    public interface IFirebaseAuthService
    {
        /// <summary>
        /// Validates <paramref name="idToken"/> against Google's public keys and returns the
        /// verified phone number in E.164 form (e.g. <c>+962791234567</c>).
        /// </summary>
        /// <returns>
        /// The phone number, or <c>null</c> when the token is missing, malformed, expired, revoked,
        /// issued for another Firebase project, or carries no <c>phone_number</c> claim. Callers
        /// should treat <c>null</c> as "not authenticated" and answer 401; the specific reason is
        /// logged server-side rather than returned, so a caller cannot probe for valid tokens.
        /// </returns>
        Task<string?> VerifyIdTokenAndGetPhoneNumberAsync(string idToken);
    }
}
