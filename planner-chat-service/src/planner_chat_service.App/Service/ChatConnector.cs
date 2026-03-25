using Microsoft.Extensions.Logging;
using planner_chat_service.Core.Entities.Models;
using planner_chat_service.Core.IRepository;
using planner_chat_service.Core.IService;
using planner_client_package.Entities;
using planner_common_package.Enums;
using planner_server_package.Converters;
using planner_server_package.Events;
using planner_server_package.Events.Enums;
using planner_server_package.RabbitMQ;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using NotificationBody = planner_server_package.Entities.NotificationBody;
using ServerNotificationSettingsBody = planner_server_package.Entities.NotificationSettingsBody;

namespace planner_chat_service.App.Service
{
    public class ChatConnector : IChatConnector
    {
        private readonly IChatRepository _chatRepository;
        private readonly IPublisherService _notifyService;
        private readonly IUserService _userService;
        private readonly ILogger<ChatConnector> _logger;
        private readonly JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true
        };

        public ChatConnector(
            IChatRepository chatRepository,
            IPublisherService notifyService,
            ILogger<ChatConnector> logger,
            IUserService userService
        )
        {
            _chatRepository = chatRepository;
            _notifyService = notifyService;
            _logger = logger;
            _userService = userService;
        }

        public async Task Invoke(
            Guid accountId,
            Chat chat,
            ChatLobby lobby,
            ChatSession currentSession,
            AccountChatSession accountChatSession
        )
        {
            await Loop(lobby, currentSession, accountChatSession, chat, accountId);
        }

        private async Task Loop(
            ChatLobby lobby,
            ChatSession currentSession,
            AccountChatSession accountChatSession,
            Chat chat,
            Guid accountId
        )
        {
            var ws = currentSession.Ws;
            string? errorMessage = null;
            DateTime? dateLastViewingMessage = null;


            var sessions = lobby.ActiveSessions.Select(e => e.Value);
            var allUserIds = lobby.AllChatUsers;
            var activeSessionIds = lobby.ActiveSessions.Select(e => e.Key);

            dateLastViewingMessage = await ProcessWebSocketState(ws, sessions, allUserIds, chat, accountId, CancellationToken.None);

            var chatMembership = await _chatRepository.GetChatSettingsAsync(chat.Id, accountId);

            await CloseWebSocket(ws, errorMessage);
            await UpdateLastViewingDate(chatMembership, accountChatSession, dateLastViewingMessage);
        }

        private async Task<DateTime?> ProcessWebSocketState(
            WebSocket ws,
            IEnumerable<ChatSession> sessions,
            IEnumerable<Guid> allUserIds,
            Chat chat,
            Guid accountId,
            CancellationToken cancellationToken
        )
        {
            DateTime? dateLastViewingMessage = null;


            try
            {
                while (ws.State == WebSocketState.Open)
                {
                    var stream = await ReceiveMessage(ws, cancellationToken);
                    if (stream == null)
                        return dateLastViewingMessage;

                    dateLastViewingMessage = await ProcessReceivedMessage(stream, sessions, allUserIds, chat, accountId);
                }
            }
            catch (JsonException e)
            {
                string message = $"{e.Message} by {accountId}";
                _logger.LogInformation(message);
            }
            catch (WebSocketException e)
            {
                string message = $"{e.Message} by {accountId}";
                _logger.LogInformation(message);
            }

            return dateLastViewingMessage;
        }

        private async Task<DateTime?> ProcessSentMessage(
            SendMessageBody sentMessage,
            IEnumerable<ChatSession> sessions,
            IEnumerable<Guid> allUserIds,
            Chat chat,
            Guid accountId
        )
        {
            //if (sentMessage.LastMessageReadId == null)
            //{
            if (sentMessage.MessageType == MessageType.File && Guid.TryParse(sentMessage.Content, out var messageId))
            {
                var message = await _chatRepository.GetMessageAsync(messageId);
                if (message != null)
                {
                    await SendMessage(sessions, message.ToNodeBody(), WebSocketMessageType.Text, allUserIds, chat);
                    return message.SentAt;
                }
            }
            else
            {
                var chatMessage = await _chatRepository.AddMessageAsync(sentMessage.MessageType, sentMessage.Content, chat, accountId, sentMessage.Id, sentMessage.SenderDeviceId);
                await SendMessage(sessions, chatMessage.ToNodeBody(), WebSocketMessageType.Text, allUserIds, chat);
                return chatMessage.SentAt;
            }
            //}

            //var lastMessage = await _chatRepository.GetMessageAsync((Guid)sentMessage.LastMessageReadId);
            //lastMessage = await _chatRepository.SetMessageIsRead(lastMessage);
            //await SendMessage(sessions, sentMessage, WebSocketMessageType.Text, allUserIds, chat);

            return sentMessage?.SentAt;
        }


        public async Task SendMessage(
            IEnumerable<ChatSession> sessions,
            MessageBody message,
            WebSocketMessageType messageType,
            IEnumerable<Guid> userIds,
            Chat chat
        )
        {

            var connectedSessionIds = sessions.Select(e => e.SessionId);
            var connectedAccountIds = sessions.GroupBy(e => e.AccountId).Select(e => e.Key);
            var notConnectedAccountIds = userIds.Except(connectedAccountIds);

            try
            {
                var user = await _userService.GetUserData(message.SenderId);

                Dictionary<string, string> data = new Dictionary<string, string>()
                {
                    { "type", NotificationType.ChatMessage.ToString() },
                    { "chatId", chat.Id.ToString() },
                    { "senderName", user.Nickname },
                    { "messageId", message.Id.ToString() },
                    { "message", message.Content },
                    { "iconUrl", user.UrlIcon }
                };

                var accountIds = new GetNotificationSettingsRequest()
                {
                    AccountIds = notConnectedAccountIds.ToList()
                };

                var response = await _notifyService.Publish(accountIds, PublishEvent.GetNotificationSettings);

                if (response?.IsSuccess == true && response.Body != null)
                {
                    if (response.Body is JsonElement jsonElement)
                    {
                        var settingsArray = JsonSerializer.Deserialize<List<ServerNotificationSettingsBody>>(jsonElement);

                        _logger.LogInformation($"Found array with {settingsArray.Count} items");

                        foreach (var x in settingsArray)
                        {
                            try
                            {
                                await _notifyService.Publish(new NotificationBody(x.AccountId, user.Nickname, message.Content, NotificationType.ChatMessage, data), PublishEvent.SendNotification);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Failed to send notification to account {x.AccountId}");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError($"Cannot cast {response.Body.GetType().Name} to List<ServerNotificationSettingsBody>");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notifications for user {SenderId}", message.SenderId);
            }

            var bytes = SerializeObject(message);
            var userSessionsDeliveryMessage = await SendMessageToConnectedUsers(sessions, bytes, messageType);
            DeliverMessageToDisconnectedUsers(notConnectedAccountIds, userSessionsDeliveryMessage, bytes);
        }

        private void DeliverMessageToDisconnectedUsers(IEnumerable<Guid> accountIds, IEnumerable<AccountSessions> accountSessions, byte[] bytes)
        {
            var messageSentToChatEvent = new MessageSentToChatEvent
            {
                AccountIds = accountIds,
                AccountSessions = accountSessions.Select(x => BodyConverter.ClientToServerBody(x)),
                Message = bytes
            };

            _notifyService.Publish(messageSentToChatEvent, PublishEvent.MessageSentToChat);
        }

        private async Task<IEnumerable<AccountSessions>> SendMessageToConnectedUsers(IEnumerable<ChatSession> sessions, byte[] bytes, WebSocketMessageType messageType)
        {
            var userSessionsDeliveryMessage = new List<AccountSessions>();

            foreach (var groupedSessions in sessions.GroupBy(e => e.AccountId))
            {
                var sessionsReceivedMessage = new List<Guid>();
                foreach (var session in groupedSessions)
                {
                    if (await SendMessageToSession(session.Ws, bytes, messageType))
                        sessionsReceivedMessage.Add(session.SessionId);
                }

                if (sessionsReceivedMessage.Any())
                    userSessionsDeliveryMessage.Add(new AccountSessions { AccountId = groupedSessions.Key, SessionIds = sessionsReceivedMessage });
            }

            return userSessionsDeliveryMessage;
        }

        private async Task<bool> SendMessageToSession(WebSocket webSocket, byte[] bytes, WebSocketMessageType messageType)
        {
            try
            {
                await webSocket.SendAsync(bytes, messageType, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                return false;
            }

            return true;
        }

        private async Task<DateTime?> ProcessReceivedMessage(
            MemoryStream stream,
            IEnumerable<ChatSession> sessions,
            IEnumerable<Guid> allUserIds,
            Chat chat,
            Guid accountId
        )
        {
            stream.Seek(0, SeekOrigin.Begin);
            var bytes = stream.GetBuffer();
            var input = Encoding.UTF8.GetString(bytes);

            if (input == null)
                return null;

            _logger.LogInformation("Received message: {Message}", input);
            input = input.Replace("\0", "");
            var sentMessage = JsonSerializer.Deserialize<SendMessageBody>(input, options);

            return await ProcessSentMessage(sentMessage, sessions, allUserIds, chat, accountId);
        }

        private async Task<MemoryStream?> ReceiveMessage(WebSocket webSocket, CancellationToken token)
        {
            byte[] bytes = new byte[4096];
            MemoryStream stream = new();

            WebSocketReceiveResult? receiveResult;
            do
            {
                receiveResult = await webSocket.ReceiveAsync(bytes, token);
                if (receiveResult.MessageType == WebSocketMessageType.Close && webSocket.State != WebSocketState.Closed)
                {
                    await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, token);
                    return null;
                }
                else if (receiveResult.Count > 0)
                    stream.Write(bytes, 0, receiveResult.Count);
            } while (!receiveResult.EndOfMessage && webSocket.State == WebSocketState.Open);

            return stream;
        }

        private async Task UpdateLastViewingDate(Core.Entities.Models.ChatSettings chatMembership, AccountChatSession accountChatSession, DateTime? dateLastViewingMessage)
        {
            if (dateLastViewingMessage != null)
            {
                var dateLastViewing = (DateTime)dateLastViewingMessage;
                await _chatRepository.UpdateLastViewingChatMembership(chatMembership, dateLastViewing);
                await _chatRepository.UpdateLastViewingUserChatSession(accountChatSession, dateLastViewing);
            }
        }

        private async Task CloseWebSocket(WebSocket ws, string? errorMessage)
        {
            if (ws.State == WebSocketState.Open)
                await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, errorMessage, CancellationToken.None);
        }

        private byte[] SerializeObject<T>(T obj)
        {
            var serializableString = JsonSerializer.Serialize(obj, options);
            var bytes = Encoding.UTF8.GetBytes(serializableString);
            return bytes;
        }
    }
}