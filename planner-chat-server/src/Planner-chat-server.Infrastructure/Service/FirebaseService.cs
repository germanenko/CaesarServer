using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Planner_chat_server.Core.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planner_chat_server.Infrastructure.Service
{
    public class FirebaseService : IFirebaseService
    {
        private readonly ILogger<FirebaseService> _logger;

        public FirebaseService(ILogger<FirebaseService> logger)
        {
            _logger = logger;

            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile("service-account-key.json"),
                    ProjectId = "caesar-e293e"
                });
                _logger.LogInformation("Firebase Admin SDK initialized");
            }
        }

        public async Task<string> SendNotificationAsync(string token, string title, string body, Dictionary<string, string>? data = null)
        {
            try
            {
                var message = new Message()
                {
                    Token = token,
                    Notification = new Notification()
                    {
                        Title = title,
                        Body = body
                    },
                    Data = data ?? new Dictionary<string, string>()
                };

                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _logger.LogInformation("Firebase notification sent successfully: {MessageId}", response);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send Firebase notification to token: {Token}", token);
                throw;
            }
        }

        public async Task<string> SendDataMessageAsync(string token, Dictionary<string, string> data)
        {
            try
            {
                var message = new Message()
                {
                    Token = token,
                    Data = data
                };

                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _logger.LogInformation("Firebase data message sent successfully: {MessageId}", response);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send Firebase data message to token: {Token}", token);
                throw;
            }
        }

        public async Task<BatchResponse> SendMulticastNotificationAsync(List<string> tokens, string title, string body, Dictionary<string, string>? data = null)
        {
            try
            {
                var message = new MulticastMessage()
                {
                    Tokens = tokens,
                    Notification = new Notification()
                    {
                        Title = title,
                        Body = body
                    },
                    Data = data ?? new Dictionary<string, string>()
                };

                var response = await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);
                _logger.LogInformation("Firebase multicast notification sent. Success: {SuccessCount}, Failure: {FailureCount}",
                    response.SuccessCount, response.FailureCount);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send Firebase multicast notification to {TokenCount} tokens", tokens.Count);
                throw;
            }
        }
    }
}
