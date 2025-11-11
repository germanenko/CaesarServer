using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using planner_notify_service.Core.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_notify_service.Infrastructure.Service
{
    public class FirebaseService : IFirebaseService
    {
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
