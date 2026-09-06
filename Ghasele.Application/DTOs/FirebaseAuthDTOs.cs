namespace Ghasele.Application.DTOs
{
    /// <summary>
    /// Body of <c>POST /api/auth/firebase-login</c>.
    /// </summary>
    /// <remarks>
    /// Firebase Phone Auth runs entirely on the client: Google sends the SMS and checks the code,
    /// so the backend never sees either. What reaches us is the signed ID token Firebase issues
    /// afterwards, which we verify and trade for one of our own JWTs. This is a separate path from
    /// the WhatsApp OTP flow in <c>AuthController</c>, which stays the fallback and is unaffected.
    /// <para>
    /// <see cref="IdToken"/> is deliberately nullable: a missing field then arrives as <c>null</c>
    /// and is answered with this API's <c>{ errorCode, message }</c> body, instead of ASP.NET's
    /// automatic model validation rejecting it first with a ProblemDetails payload that the mobile
    /// clients do not parse.
    /// </para>
    /// </remarks>
    /// <param name="IdToken">
    /// The token from <c>FirebaseAuth.instance.currentUser!.getIdToken()</c> on the client. It is
    /// short-lived (one hour) and is re-verified against Google's public keys on every call, so a
    /// stale or tampered token is rejected rather than trusted.
    /// </param>
    public record FirebaseLoginRequest(string? IdToken);

    /// <summary>
    /// Body of <c>POST /api/auth/google</c>.
    /// </summary>
    /// <remarks>
    /// Google sign-in also runs entirely on the client: the app obtains a Google credential, hands
    /// it to Firebase, and sends us the Firebase ID token that comes back. So this carries the same
    /// kind of token as <see cref="FirebaseLoginRequest"/> - the difference is which claims it
    /// holds (an email rather than a phone number), which is why it gets its own endpoint instead
    /// of overloading the phone one.
    /// </remarks>
    /// <param name="IdToken">
    /// From <c>FirebaseAuth.instance.currentUser!.getIdToken()</c> after
    /// <c>signInWithCredential(GoogleAuthProvider.credential(...))</c>. Nullable for the same
    /// reason as <see cref="FirebaseLoginRequest.IdToken"/>.
    /// </param>
    public record GoogleLoginRequest(string? IdToken);

    /// <summary>
    /// Body of <c>POST /api/auth/firebase-complete-registration</c>.
    /// </summary>
    /// <remarks>
    /// Firebase phone sign-in proves the number but sets no password, which left accounts that
    /// could never sign in through <c>POST /api/auth/signin</c>. This carries the verified token
    /// together with the name and password the user chooses, so the account is created complete.
    /// </remarks>
    public record FirebaseCompleteRegistrationRequest(string? IdToken, string? FullName, string? Password);
}
