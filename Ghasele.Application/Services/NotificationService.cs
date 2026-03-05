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
            try
            {
                var path = configuration["Firebase:CredentialsPath"];
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    if (FirebaseApp.DefaultInstance == null)
                    {
                        FirebaseApp.Create(new AppOptions()
                        {
                            Credential = GoogleCredential.FromFile(path),
                        });
                    }
                    _isFirebaseInitialized = true;
                }
                else
                {
                    Console.WriteLine("Firebase Credentials not found. Notifications will be simulated.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing Firebase: {ex.Message}");
            }
        }

        public async Task SendNotificationAsync(string fcmToken, string title, string body)
        {
            if (string.IsNullOrEmpty(fcmToken)) return;

            if (!_isFirebaseInitialized)
            {
                Console.WriteLine($"[SIMULATED NOTIFICATION] To: {fcmToken} | Title: {title} | Body: {body}");
                return;
            }

            try
            {
                var message = new Message()
                {
                    Token = fcmToken,
                    Notification = new Notification()
                    {
                        Title = title,
                        Body = body,
                    },
                    Data = new System.Collections.Generic.Dictionary<string, string>()
                    {
                        { "click_action", "FLUTTER_NOTIFICATION_CLICK" },
                    }
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
