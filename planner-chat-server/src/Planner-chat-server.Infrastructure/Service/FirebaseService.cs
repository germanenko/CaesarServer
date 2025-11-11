using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Planner_chat_server.Core.Entities.Response;
using Planner_chat_server.Core.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Planner_chat_server.Infrastructure.Service
{
    public class FirebaseService : IFirebaseService
    {
        private readonly HttpClient _httpClient;
        public FirebaseService(string fbProjectId, string fbClientEmail, string fbPrivateKey)
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                var formattedPrivateKey = fbPrivateKey?.Replace("\\n", "\n");

                var credential = GoogleCredential.FromJson($$"""
                    {
                        "type": "service_account",
                        "project_id": "{{fbProjectId}}",
                        "private_key": "{{formattedPrivateKey}}",
                        "client_email": "{{fbClientEmail}}"
                    }
                    """);


                FirebaseApp.Create(new AppOptions()
                {
                    Credential = credential,
                    ProjectId = fbProjectId
                });

                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };

                _httpClient = new HttpClient(handler)
                {
                    BaseAddress = new Uri("https://planner-notify-service:8092/api/")
                };
            }
        }

        public async Task<string> SendNotification(Guid userId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"sendFCMNotification", null);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    var user = JsonSerializer.Deserialize<ProfileBody>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var userName = user?.Nickname ?? userId.ToString();

                    return userName;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("❌ HTTP {StatusCode}: {Error}", response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Unexpected error for {UserId}", userId);
            }

            return userId.ToString();
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
                return response;
            }
            catch (Exception ex)
            {
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
                return response;
            }
            catch (Exception ex)
            {
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

                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
