using Microsoft.Extensions.Logging;
using planner_chat_service.Core.Entities.Models;
using planner_chat_service.Core.Entities.Request;
using planner_chat_service.Core.IRepository;
using planner_chat_service.Core.IService;
using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_common_package.Enums;
using planner_server_package;
using planner_server_package.Access;
using planner_server_package.Converters;
using planner_server_package.Events;
using planner_server_package.Events.Enums;
using planner_server_package.RabbitMQ;
using System.Net;
using System.Net.WebSockets;

namespace planner_chat_service.App.Service
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IChatConnectionService _chatConnectionService;
        private readonly IChatConnector _chatConnector;
        private readonly IPublisherService _notifyService;
        private readonly IUserService _userService;
        private readonly IAccessService _accessService;
        private readonly ILogger<ChatService> _logger;

        public ChatService(
            IChatRepository chatRepository,
            IChatConnectionService chatConnectionService,
            IChatConnector chatConnector,
            IPublisherService notifyService,
            IUserService userService,
            IAccessService accessService,
            ILogger<ChatService> logger)
        {
            _chatRepository = chatRepository;
            _chatConnectionService = chatConnectionService;
            _chatConnector = chatConnector;
            _notifyService = notifyService;
            _userService = userService;
            _accessService = accessService;
            _logger = logger;
        }

        public async Task ConnectToChat(Guid accountId, Guid chatId, WebSocket socket, Guid sessionId)
        {
            var chat = await _chatRepository.GetAsync(chatId);
            if (chat == null)
                return;

            var session = new ChatSession
            {
                AccountId = accountId,
                Ws = socket,
                SessionId = sessionId
            };

            var chatMembership = await _chatRepository.GetChatSettingsAsync(chatId, accountId);

            var hasAccess = await _accessService.CheckAccess(accountId, chatId, Permission.Read);

            if (chatMembership == null)
                return;

            var accountChatSession = await _chatRepository.CreateOrGetAccountChatSessionAsync(sessionId, chatMembership.Id, chatMembership.DateLastViewing);
            if (accountChatSession == null)
                return;

            if (!_chatConnectionService.LobbyIsExist(chatId))
            {
                var chatMemberships = await _chatRepository.GetChatSettingsAsync(chatId);
                var userIds = chatMemberships.Select(e => e.AccountId).ToList();
                _chatConnectionService.AddLobby(chatId, userIds);
            }

            var lobby = _chatConnectionService.AddSessionToLobby(chatId, session);
            if (lobby == null)
                return;

            try
            {
                await _chatConnector.Invoke(accountId, chat, lobby, session, accountChatSession);
            }
            finally
            {
                _chatConnectionService.RemoveConnection(chatId, session);
            }
        }

        public async Task<ServiceResponse<ChatBody>> CreatePersonalChat(
            Guid accountId,
            Guid sessionId,
            CreateChatBody createChatBody)
        {
            var participants = new List<Guid>
            {
                accountId,
                createChatBody.CompanionId
            };

            var currentDate = DateTime.UtcNow;
            var result = await _chatRepository.AddPersonalChatAsync(accountId, participants, createChatBody, currentDate);
            if (result == null)
                return new ServiceResponse<ChatBody>
                {
                    StatusCode = HttpStatusCode.Conflict,
                    IsSuccess = false,
                    ErrorCodes = [ErrorCode.AlreadyExist],
                    Errors = ["chat exist"]
                };

            var createChatEvent = new CreatePersonalChatEvent
            {
                Chat = BodyConverter.ClientToServerBody(result),
                ParticipantIds = [accountId, createChatBody.CompanionId]
            };

            var companion = await _userService.GetUserData(createChatBody.CompanionId);

            _notifyService.Publish(createChatEvent, PublishEvent.CreatePersonalChat);
            _notifyService.Publish(createChatEvent, PublishEvent.AddAccountToChat);
            return new ServiceResponse<ChatBody>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = result
            };
        }

        public async Task<ServiceResponse<IEnumerable<ChatBody>>> GetChats(Guid accountId, Guid sessionId, ChatType chatType)
        {
            var chats = await _chatRepository.GetChatBodies(accountId, sessionId, chatType);

            foreach (var chat in chats)
            {
                if (chat.ChatType == ChatType.Personal)
                {
                    var user = await _userService.GetUserData(chat.ParticipantIds.FirstOrDefault());
                    chat.ImageUrl = user.UrlIcon;
                    chat.Profile = user;
                    chat.Name = user.Nickname;
                }
            }

            return new ServiceResponse<IEnumerable<ChatBody>>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = chats
            };
        }

        public async Task<ServiceResponse<ChatBody>> GetChat(Guid accountId, Guid userSessionId, Guid chatId)
        {
            var chat = await _chatRepository.GetChat(accountId, userSessionId, chatId);

            chat.Profile = await _userService.GetUserData(chat.ParticipantIds.FirstOrDefault());

            return new ServiceResponse<ChatBody>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = chat
            };
        }

        public async Task<ServiceResponse<IEnumerable<MessageBody>>> GetMessages(Guid accountId, Guid chatId, DynamicDataLoadingOptions options)
        {
            var messages = await _chatRepository.GetMessagesAsync(chatId, options.Count, options.LoadPosition);
            return new ServiceResponse<IEnumerable<MessageBody>>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = messages.Select(e => e.ToNodeBody())
            };
        }

        public async Task<ServiceResponse<MessageBody>> EditMessage(Guid accountId, EditMessageBody updatedMessage)
        {
            var existingMessage = await _chatRepository.GetMessageAsync(updatedMessage.MessageId);

            if (existingMessage == null)
            {
                return new ServiceResponse<MessageBody>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    Errors = new[] { "Сообщения не существует" }
                };
            }

            var hasAccess = await _accessService.CheckAccess(accountId, existingMessage.Id, Permission.Write);

            if (hasAccess)
            {
                var message = await _chatRepository.UpdateMessage(accountId, updatedMessage);

                if (message == null)
                {
                    return new ServiceResponse<MessageBody>
                    {
                        StatusCode = HttpStatusCode.InternalServerError,
                        IsSuccess = false,
                        Errors = new[] { "Не удалось обновить сообщение" }
                    };
                }

                var messageEditedEvent = new MessageEditedEvent(message.Id, MessageState.Edited);

                await _notifyService.Publish(messageEditedEvent, PublishEvent.MessageEdited);

                return new ServiceResponse<MessageBody>
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true,
                    Body = message.ToNodeBody()
                };
            }

            return new ServiceResponse<MessageBody>
            {
                StatusCode = HttpStatusCode.Forbidden,
                IsSuccess = false,
                Errors = new[] { "Нет доступа" }
            };
        }

        public async Task<ServiceResponse<MessageBody>> DeleteMessage(Guid accountId, Guid messageId)
        {
            var existingMessage = await _chatRepository.GetMessageAsync(messageId);

            if (existingMessage == null)
            {
                return new ServiceResponse<MessageBody>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    Errors = new[] { "Сообщения не существует" }
                };
            }

            var hasAccess = await _accessService.CheckAccess(accountId, existingMessage.Id, Permission.Write);

            if (hasAccess)
            {
                var message = await _chatRepository.DeleteMessage(accountId, messageId);

                if (message == null)
                {
                    return new ServiceResponse<MessageBody>
                    {
                        StatusCode = HttpStatusCode.InternalServerError,
                        IsSuccess = false,
                        Errors = new[] { "Не удалось удалить сообщение" }
                    };
                }

                var messageEditedEvent = new MessageEditedEvent(message.Id, MessageState.Deleted);

                await _notifyService.Publish(messageEditedEvent, PublishEvent.MessageEdited);

                return new ServiceResponse<MessageBody>
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true,
                    Body = message.ToNodeBody()
                };
            }

            return new ServiceResponse<MessageBody>
            {
                StatusCode = HttpStatusCode.Forbidden,
                IsSuccess = false,
                Errors = new[] { "Нет доступа" }
            };
        }

        public async Task<ServiceResponse<MessageBody>> DeleteMessageForMe(Guid accountId, Guid messageId)
        {
            var existingMessage = await _chatRepository.GetMessageAsync(messageId);

            if (existingMessage == null)
            {
                return new ServiceResponse<MessageBody>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    Errors = new[] { "Сообщения не существует" }
                };
            }

            var hasAccess = await _accessService.CheckAccess(accountId, existingMessage.Id, Permission.Write);

            if (hasAccess)
            {
                var message = await _chatRepository.DeleteMessageForMe(accountId, messageId);

                if (message == null)
                {
                    return new ServiceResponse<MessageBody>
                    {
                        StatusCode = HttpStatusCode.InternalServerError,
                        IsSuccess = false,
                        Errors = new[] { "Не удалось удалить сообщение" }
                    };
                }

                var messageEditedEvent = new MessageEditedEvent(message.Id, MessageState.Deleted);

                await _notifyService.Publish(messageEditedEvent, PublishEvent.MessageEdited);

                return new ServiceResponse<MessageBody>
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true,
                    Body = message.ToNodeBody()
                };
            }

            return new ServiceResponse<MessageBody>
            {
                StatusCode = HttpStatusCode.Forbidden,
                IsSuccess = false,
                Errors = new[] { "Нет доступа" }
            };
        }

        public async Task<ServiceResponse<MessageBody>> SendMessage(
            Guid senderId, Guid? senderDeviceId, Guid receiverId, string content)
        {
            var chat = await _chatRepository.GetPersonalChatAsync(senderId, receiverId);

            if (chat == null)
            {
                return new ServiceResponse<MessageBody>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    Errors = new[] { "Chat not found" }
                };
            }

            var message = await _chatRepository.AddMessageAsync(MessageType.Mail, content, chat.Chat, senderId, Guid.NewGuid(), null);

            ChatLobby lobby = _chatConnectionService.GetConnections(chat.ChatId);

            if (!_chatConnectionService.LobbyIsExist(chat.ChatId))
            {
                var chatMemberships = await _chatRepository.GetChatSettingsAsync(chat.ChatId);
                var userIds = chatMemberships.Select(e => e.AccountId).ToList();
                lobby = _chatConnectionService.AddLobby(chat.ChatId, userIds);
            }

            var messageBody = message.ToNodeBody();

            await _chatConnector.SendMessage(lobby.ActiveSessions.Values, messageBody, WebSocketMessageType.Text, lobby.AllChatUsers, chat.Chat);

            _chatConnectionService.RemoveLobby(chat.ChatId);

            return new ServiceResponse<MessageBody>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = message.ToNodeBody()
            };
        }

        public async Task<ServiceResponse<MessageBody>> SendMessageToChat(
            Guid senderId, Guid? senderDeviceId, Guid chatId, string content)
        {
            var chat = (await _chatRepository.GetChatSettingsAsync(chatId)).FirstOrDefault();

            if (chat == null)
            {
                return new ServiceResponse<MessageBody>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    Errors = new[] { "Chat not found" }
                };
            }

            var message = await _chatRepository.AddMessageAsync(MessageType.Mail, content, chat.Chat, senderId, Guid.NewGuid(), senderDeviceId);

            ChatLobby? lobby = _chatConnectionService.GetConnections(chat.ChatId);

            if (lobby == null)
            {
                return new ServiceResponse<MessageBody>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    Errors = new[] { "Lobby not found" }
                };
            }

            if (!_chatConnectionService.LobbyIsExist(chat.ChatId))
            {
                var chatMemberships = await _chatRepository.GetChatSettingsAsync(chat.ChatId);
                var userIds = chatMemberships.Select(e => e.AccountId).ToList();
                lobby = _chatConnectionService.AddLobby(chat.ChatId, userIds);
            }

            await _chatConnector.SendMessage(lobby.ActiveSessions.Values, message.ToNodeBody(), WebSocketMessageType.Text, lobby.AllChatUsers, chat.Chat);

            _chatConnectionService.RemoveLobby(chat.ChatId);

            return new ServiceResponse<MessageBody>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = message.ToNodeBody()
            };
        }

        public async Task<ServiceResponse<bool>> CreateOrUpdateMessageDraft(Guid accountId, Guid chatId, string content)
        {
            var membership = await _chatRepository.GetChatSettingsAsync(chatId, accountId);

            if (membership == null)
            {
                return new ServiceResponse<bool>
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    IsSuccess = false,
                    Errors = new[] { "�� �� �������� ����� ����" }
                };
            }

            var result = await _chatRepository.CreateOrUpdateMessageDraft(membership, content);

            return new ServiceResponse<bool>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = result
            };
        }

        public async Task<ServiceResponse<List<ChatSettings>>> GetChatsSettings(Guid accountId)
        {
            var settings = await _chatRepository.GetChatSettingsByAccountIdAsync(accountId);

            return new ServiceResponse<List<ChatSettings>>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = settings
            };
        }
    }
}