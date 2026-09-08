using System.Threading.Tasks;

namespace Ghasele.Application.Interfaces
{
    /// <summary>
    /// The verified identity carried by a Firebase ID token.
    /// </summary>
    /// <remarks>
    /// Which fields are populated depends on how the client signed in: phone sign-in yields a
    /// <see cref="PhoneNumber"/> and no email, Google sign-in the reverse. <see cref="Uid"/> is the
    /// only field Firebase always sets.
    /// </remarks>
    /// <param name="Uid">Firebase's stable per-project user id.</param>
    /// <param name="PhoneNumber">E.164 phone number, when the token came from phone sign-in.</param>
    /// <param name="Email">Email address, when the token came from a provider that carries one.</param>
    /// <param name="EmailVerified">
    /// Whether Firebase considers <see cref="Email"/> proven. Google always sets this; it is what
    /// makes matching an existing account by email safe rather than a takeover vector.
    /// </param>
    /// <param name="Name">Display name, when the provider supplied one.</param>
    public record FirebaseIdentity(
        string Uid,
        string? PhoneNumber,
        string? Email,
        bool EmailVerified,
        string? Name);

    /// <summary>
    /// Verifies Firebase ID tokens produced by client-side Firebase Authentication.
    /// </summary>
    /// <remarks>
    /// Deliberately narrow: this type answers only "is this token genuine, and whose identity does
    /// it carry?". Deciding what to do with that identity - find the user, create one, issue a JWT -
    /// is account policy and lives in <see cref="IAuthService"/>, so this stays a thin, testable
    /// seam over the Firebase SDK.
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

        /// <summary>
        /// Validates <paramref name="idToken"/> and returns everything it proves about the caller.
        /// </summary>
        /// <remarks>
        /// Added for Google sign-in, whose tokens carry an email and no phone number.
        /// <see cref="VerifyIdTokenAndGetPhoneNumberAsync"/> is left in place unchanged so the phone
        /// sign-in path is not disturbed by this.
        /// </remarks>
        /// <returns>The identity, or <c>null</c> when the token is not usable - same policy as above.</returns>
        Task<FirebaseIdentity?> VerifyIdTokenAsync(string idToken);
    }
}
