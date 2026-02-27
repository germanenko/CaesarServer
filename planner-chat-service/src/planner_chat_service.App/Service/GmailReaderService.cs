using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using planner_chat_service.Core.IService;
using planner_server_package.Entities;
using planner_server_package.Events.Enums;
using System.Text.Json;

namespace planner_chat_service.App.Service
{
    public class GmailReaderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<GmailReaderService> _logger;
        public GmailReaderService(IServiceScopeFactory serviceFactory, ILogger<GmailReaderService> logger)
        {
            _scopeFactory = serviceFactory;
            _logger = logger;
        }

        private static string ApplicationName = "Gmail Service API .NET Quickstart";

        private string ProjectId;
        private string SubscriptionId;

        private UserCredential _userCredential;
        private GoogleCredential _serviceCredential;

        string GetEnvVar(string name) => Environment.GetEnvironmentVariable(name) ?? throw new Exception($"{name} is not set");

        public async Task Main()
        {
            ProjectId = GetEnvVar("PROJECT_ID");
            SubscriptionId = GetEnvVar("SUBSCRIPTION_ID");

            using (var stream = new FileStream("key.json", FileMode.Open, FileAccess.Read))
            {
                _serviceCredential = await GoogleCredential.FromStreamAsync(stream, CancellationToken.None);
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            var sub = StartSubscriber(ProjectId, SubscriptionId, true, cts.Token);

            Console.CancelKeyPress += (s, e) => { e.Cancel = true; cts.Cancel(); };

            try
            {
                await Task.Delay(Timeout.Infinite, cts.Token);
            }
            catch (TaskCanceledException) { }

            await sub.StopAsync(CancellationToken.None);
        }



        public SubscriberClient StartSubscriber(string projectId, string subscriptionId, bool acknowledge, CancellationToken cancellationToken)
        {
            SubscriptionName subscriptionName = SubscriptionName.FromProjectSubscription(projectId, subscriptionId);

            var subscriberClientBuilder = new SubscriberClientBuilder
            {
                SubscriptionName = subscriptionName,
                GoogleCredential = _serviceCredential.CreateScoped(new[] { "https://www.googleapis.com/auth/pubsub" })
            };

            SubscriberClient subscriber = subscriberClientBuilder.Build();

            subscriber.StartAsync(async (PubsubMessage message, CancellationToken ct) =>
            {
                string text = message.Data.ToStringUtf8();

                _logger.LogInformation($"\nMessage {message.MessageId}: {text}, {message.PublishTime.ToDateTime()}\n");

                string historyId = "";
                string emailAddress = "";

                using var doc = System.Text.Json.JsonDocument.Parse(text);
                if (doc.RootElement.TryGetProperty("historyId", out var hElem))
                {
                    historyId = hElem.GetRawText().Trim('"');
                }

                if (doc.RootElement.TryGetProperty("emailAddress", out var eElem))
                {
                    emailAddress = eElem.GetRawText().Trim('"');
                }

                ServiceResponse<object> googleToken = new ServiceResponse<object>();

                using (var scope = _scopeFactory.CreateScope())
                {
                    var notifyService = scope.ServiceProvider.GetRequiredService<INotifyService>();

                    googleToken = await notifyService.Publish(emailAddress, PublishEvent.GetGoogleToken);
                }

                GoogleTokenBody googleTokenBody = new GoogleTokenBody();
                if (googleToken?.IsSuccess == true && googleToken.Body != null)
                {
                    if (googleToken.Body is JsonElement jsonElement)
                    {
                        googleTokenBody = JsonSerializer.Deserialize<GoogleTokenBody>(jsonElement);
                    }
                }

                string refreshToken = googleTokenBody.RefreshToken;

                var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = GetEnvVar("GOOGLE_MAIL_READER_CLIENT_ID"),
                        ClientSecret = GetEnvVar("GOOGLE_MAIL_READER_CLIENT_SECRET")
                    },
                    Scopes = new[] { GmailService.Scope.GmailReadonly },
                    DataStore = new FileDataStore("GmailAPI")
                });

                _userCredential = new UserCredential(flow, "user", new TokenResponse
                {
                    RefreshToken = refreshToken
                });

                //_userCredential = GoogleCredential.FromAccessToken(gToken);

                var gmailService = new GmailService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = _userCredential,
                    ApplicationName = ApplicationName,
                });

                try
                {
                    var m = await GetMessageByHistoryId(gmailService, ulong.Parse(historyId));
                    _logger.LogInformation("Кому: " + m.Payload.Headers.Where(x => x.Name == "To").First().Value);
                    _logger.LogInformation("Тема сообщения: " + m.Payload.Headers.Where(x => x.Name == "Subject").First().Value);
                    _logger.LogInformation("Тело сообщения: " + m.Snippet);

                    var receiverEmail = m.Payload.Headers.Where(x => x.Name == "To").First().Value;

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                        var chatService = scope.ServiceProvider.GetRequiredService<IChatService>();

                        var receiver = await userService.GetUserData(receiverEmail);

                        if (receiver != null)
                        {
                            _logger.LogInformation($"Sending from email to chat. Sender: {googleTokenBody.AccountId}, Receiver: {JsonSerializer.Serialize(receiver)}");
                            var result = await chatService.SendMessage(googleTokenBody.AccountId, null, receiver.Id, $"Тема письма: {m.Payload.Headers.Where(x => x.Name == "Subject").First().Value}\n{m.Snippet}");
                            _logger.LogInformation($"sending result: {JsonSerializer.Serialize(result)}");
                        }
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }

                return await Task.FromResult(acknowledge ? SubscriberClient.Reply.Ack : SubscriberClient.Reply.Nack);
            });

            return subscriber;
        }



        async Task<Message> GetMessageByHistoryId(GmailService gmailService, ulong startHistoryId)
        {
            History history = null;

            while (history == null)
            {
                var l = gmailService.Users.History.List("me");
                l.StartHistoryId = startHistoryId;
                l.LabelId = "SENT";
                var a = l.Execute();
                if (a.History != null)
                {
                    history = a.History[0];
                }
                else
                {
                    await Task.Delay(1000);
                }
            }

            var m = gmailService.Users.Messages.Get("me", history.Messages[0].Id).Execute();
            return m;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            await Main();
        }
    }
}

