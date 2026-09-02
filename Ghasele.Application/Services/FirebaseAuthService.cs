using System;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Ghasele.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ghasele.Application.Services
{
    /// <summary>
    /// <see cref="IFirebaseAuthService"/> backed by the Firebase Admin SDK.
    /// </summary>
    /// <remarks>
    /// Sits beside <see cref="NotificationService"/> rather than in the Infrastructure project
    /// because that is where this solution already keeps its Firebase SDK usage, and the
    /// <c>FirebaseAdmin</c> package reference lives on this project.
    /// <para>
    /// The SDK is initialized once in <c>Program.cs</c>, not here: <c>FirebaseApp.Create</c> throws
    /// if a default app already exists, and both this service and the FCM notification service
    /// share that single instance.
    /// </para>
    /// </remarks>
    public class FirebaseAuthService : IFirebaseAuthService
    {
        /// <summary>Set by Firebase only after it has itself verified the SMS code.</summary>
        private const string PhoneNumberClaim = "phone_number";

        private readonly ILogger<FirebaseAuthService> _logger;

        public FirebaseAuthService(ILogger<FirebaseAuthService> logger)
        {
            _logger = logger;
        }

        public async Task<string?> VerifyIdTokenAndGetPhoneNumberAsync(string idToken)
        {
            if (string.IsNullOrWhiteSpace(idToken))
            {
                _logger.LogWarning("Firebase login attempted with an empty ID token.");
                return null;
            }

            // Guard explicitly rather than letting DefaultInstance throw a NullReferenceException:
            // a missing Firebase:CredentialsPath is a server misconfiguration, and the log line
            // needs to say so plainly instead of surfacing as an opaque 500.
            if (FirebaseApp.DefaultInstance == null)
            {
                _logger.LogError(
                    "Firebase Admin SDK is not initialized - check Firebase:CredentialsPath. " +
                    "Rejecting the Firebase login attempt.");
                return null;
            }

            FirebaseToken decoded;
            try
            {
                // checkRevoked: true costs one extra round trip to Google but is what makes a
                // disabled account or a signed-out-everywhere session stop working immediately.
                // Without it a leaked token stays usable for the rest of its hour.
                decoded = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken, checkRevoked: true);
            }
            catch (FirebaseAuthException ex)
            {
                // Expired, malformed, revoked, or issued for a different Firebase project. All of
                // these are "this caller is not authenticated" - the distinction is only useful in
                // our own logs, so it is recorded here and not returned.
                _logger.LogWarning(
                    ex,
                    "Firebase ID token rejected. AuthErrorCode={AuthErrorCode}, ErrorCode={ErrorCode}",
                    ex.AuthErrorCode,
                    ex.ErrorCode);
                return null;
            }
            catch (Exception ex)
            {
                // Network failure reaching Google, clock skew, a malformed service account file.
                // Still not a usable login, but distinct from a rejected token, so log louder.
                _logger.LogError(ex, "Unexpected failure while verifying a Firebase ID token.");
                return null;
            }

            // Claims is a read-only dictionary of object, so the cast has to be checked - a token
            // from an email or anonymous sign-in simply has no phone_number at all.
            if (!decoded.Claims.TryGetValue(PhoneNumberClaim, out var rawPhoneNumber) ||
                rawPhoneNumber is not string phoneNumber ||
                string.IsNullOrWhiteSpace(phoneNumber))
            {
                _logger.LogWarning(
                    "Firebase ID token for uid {Uid} verified but carries no phone_number claim; " +
                    "it was probably not issued by phone sign-in.",
                    decoded.Uid);
                return null;
            }

            // Firebase always emits E.164 (+<country><number>). Trimming only guards against
            // whitespace; the value is otherwise passed through untouched so it matches the format
            // the apps already store.
            return phoneNumber.Trim();
        }
    }
}
