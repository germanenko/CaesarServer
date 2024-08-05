using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Planner_chat_server.Core.Entities.Events;
using Planner_chat_server.Core.Entities.Models;
using Planner_chat_server.Core.Entities.Request;
using Planner_chat_server.Core.Entities.Response;
using Planner_chat_server.Core.Enums;
using Planner_chat_server.Core.IRepository;
using Planner_chat_server.Core.IService;

namespace Planner_chat_server.App.Service
{
    public class ChatConnector : IChatConnector
    {
        private readonly IChatRepository _chatRepository;
        private readonly INotifyService _notifyService;
        private readonly ILogger<ChatConnector> _logger;
        private readonly JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true
        };

        public ChatConnector(
            IChatRepository chatRepository,
            INotifyService notifyService,
            ILogger<ChatConnector> logger
        )
        {
            _chatRepository = chatRepository;
            _notifyService = notifyService;
            _logger = logger;
        }

        public async Task Invoke(
            Core.Entities.Models.ChatMembership chatMembership,
            Chat chat,
            ChatLobby lobby,
            ChatSession currentSession,
            AccountChatSession accountChatSession
        )
        {
            await Loop(lobby, currentSession, accountChatSession, chat, chatMembership);
        }

        private async Task Loop(
            ChatLobby lobby,
            ChatSession currentSession,
            AccountChatSession accountChatSession,
            Chat chat,
            Core.Entities.Models.ChatMembership chatMembership
        )
        {
            var ws = currentSession.Ws;
            string? errorMessage = null;
            DateTime? dateLastViewingMessage = null;


            var sessions = lobby.ActiveSessions.Select(e => e.Value);
            var allUserIds = lobby.AllChatUsers;
            var activeSessionIds = lobby.ActiveSessions.Select(e => e.Key);

            dateLastViewingMessage = await ProcessWebSocketState(ws, sessions, allUserIds, chat, chatMembership.AccountId, CancellationToken.None);

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
            SentMessage sentMessage,
            IEnumerable<ChatSession> sessions,
            IEnumerable<Guid> allUserIds,
            Chat chat,
            Guid accountId
        )
        {
            if (sentMessage.LastMessageReadId == null)
            {
                var messageBody = sentMessage.MessageBody;
                if (messageBody.Type == MessageType.File && Guid.TryParse(messageBody.Content, out var messageId))
                {
                    var message = await _chatRepository.GetMessageAsync(messageId);
                    if (message != null)
                    {
                        await SendMessage(sessions, message.ToMessageBody(), WebSocketMessageType.Text, allUserIds, chat);
                        return message.SentAt;
                    }
                }
                else
                {
                    var chatMessage = await _chatRepository.AddMessageAsync(messageBody.Type, messageBody.Content, chat, accountId);
                    await SendMessage(sessions, chatMessage.ToMessageBody(), WebSocketMessageType.Text, allUserIds, chat);
                    return chatMessage.SentAt;
                }
            }

            var lastMessage = await _chatRepository.GetMessageAsync((Guid)sentMessage.LastMessageReadId);
            return lastMessage?.SentAt;
        }


        public async Task SendMessage(
            IEnumerable<ChatSession> sessions,
            MessageBody message,
            WebSocketMessageType messageType,
            IEnumerable<Guid> userIds,
            Chat chat
        )
        {
            var chatMessageBody = new ChatMessageInfo
            {
                ChatId = chat.Id,
                ChatType = Enum.Parse<ChatType>(chat.Type),
                Message = message
            };

            var connectedSessionIds = sessions.Select(e => e.SessionId);
            var connectedAccountIds = sessions.GroupBy(e => e.AccountId).Select(e => e.Key);
            var notConnectedAccountIds = userIds.Except(connectedAccountIds);

            var bytes = SerializeObject(chatMessageBody);
            var userSessionsDeliveryMessage = await SendMessageToConnectedUsers(sessions, bytes, messageType);
            DeliverMessageToDisconnectedUsers(notConnectedAccountIds, userSessionsDeliveryMessage, bytes);
        }

        private void DeliverMessageToDisconnectedUsers(IEnumerable<Guid> accountIds, IEnumerable<AccountSessions> accountSessions, byte[] bytes)
        {
            var messageSentToChatEvent = new MessageSentToChatEvent
            {
                AccountIds = accountIds,
                AccountSessions = accountSessions,
                Message = bytes
            };

            _notifyService.Publish(messageSentToChatEvent, NotifyPublishEvent.MessageSentToChat);
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
            var sentMessage = JsonSerializer.Deserialize<SentMessage>(input, options);

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

        private async Task UpdateLastViewingDate(Core.Entities.Models.ChatMembership chatMembership, AccountChatSession accountChatSession, DateTime? dateLastViewingMessage)
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