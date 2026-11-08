using System;
using System.IO;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Ghasele.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Ghasele.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly bool _isFirebaseInitialized;

        public NotificationService(IConfiguration configuration)
        {
            // Program.cs creates the default Firebase app, resolving the credentials path
            // against the content root. Checking that app - rather than re-testing the raw
            // relative path here, which resolves against the process's working directory -
            // keeps pushes from silently falling back to "simulated" on a host whose service
            // starts in a different directory from the one the app is deployed to.
            _isFirebaseInitialized = FirebaseApp.DefaultInstance != null;
            if (!_isFirebaseInitialized)
            {
                Console.WriteLine("Firebase Credentials not found. Notifications will be simulated.");
            }
        }

        public async Task SendNotificationAsync(string fcmToken, string title, string body, System.Collections.Generic.Dictionary<string, string>? data = null)
        {
            if (string.IsNullOrEmpty(fcmToken)) return;

            if (!_isFirebaseInitialized)
            {
                Console.WriteLine($"[SIMULATED NOTIFICATION] To: {fcmToken} | Title: {title} | Body: {body} | Data: {(data == null ? "{}" : string.Join(", ", data))}");
                return;
            }

            try
            {
                var payload = new System.Collections.Generic.Dictionary<string, string>()
                {
                    { "click_action", "FLUTTER_NOTIFICATION_CLICK" },
                };
                if (data != null)
                {
                    foreach (var kvp in data) payload[kvp.Key] = kvp.Value;
                }

                var message = new Message()
                {
                    Token = fcmToken,
                    Notification = new Notification()
                    {
                        Title = title,
                        Body = body,
                    },
                    Data = payload
                };

                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                Console.WriteLine($"Successfully sent message: {response}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending FCM notification: {ex.Message}");
            }
        }
    }
}
