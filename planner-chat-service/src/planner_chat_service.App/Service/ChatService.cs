using Microsoft.Extensions.Logging;
using planner_chat_service.Core.Entities.Models;
using planner_chat_service.Core.Entities.Request;
using planner_chat_service.Core.IRepository;
using planner_chat_service.Core.IService;
using planner_server_package.Entities;
using planner_server_package.Enums;
using planner_server_package.Events;
using System.Net;
using System.Net.WebSockets;

namespace planner_chat_service.App.Service
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IChatConnectionService _chatConnectionService;
        private readonly IChatConnector _chatConnector;
        private readonly INotifyService _notifyService;
        private readonly IUserService _userService;
        private readonly ILogger<ChatService> _logger;

        public ChatService(
            IChatRepository chatRepository,
            IChatConnectionService chatConnectionService,
            IChatConnector chatConnector,
            INotifyService notifyService,
            IUserService userService,
            ILogger<ChatService> logger)
        {
            _chatRepository = chatRepository;
            _chatConnectionService = chatConnectionService;
            _chatConnector = chatConnector;
            _notifyService = notifyService;
            _userService = userService;
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
            var access = await _chatRepository.GetChatAccess(accountId, chatId);

            if (chatMembership == null || access == null)
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
                await _chatConnector.Invoke(access, chat, lobby, session, accountChatSession);
            }
            finally
            {
                _chatConnectionService.RemoveConnection(chatId, session);
            }
        }

        public async Task<ServiceResponse<ChatBody>> CreatePersonalChat(
            Guid accountId,
            Guid sessionId,
            ChatBody createChatBody,
            Guid addedAccountId)
        {
            var participants = new List<Guid>
            {
                accountId,
                addedAccountId
            };

            var currentDate = DateTime.UtcNow;
            var result = await _chatRepository.AddPersonalChatAsync(participants, createChatBody, currentDate);
            if (result == null)
                return new ServiceResponse<ChatBody>
                {
                    StatusCode = HttpStatusCode.Conflict,
                    IsSuccess = false,
                    Errors = new string[] { "chat exist" }
                };

            var chatMemberships = new List<ChatMembership>();
            foreach (var chatMembership in result.ChatMemberships)
            {
                chatMemberships.Add(new ChatMembership
                {
                    AccountId = chatMembership.AccountId,
                    ChatMembershipId = chatMembership.Id
                });
            }

            var createChatEvent = new CreatePersonalChatEvent
            {
                Chat = result.ToChatBody(),
                Participants = chatMemberships
            };

            _notifyService.Publish(createChatEvent, NotifyPublishEvent.CreatePersonalChat);
            _notifyService.Publish(createChatEvent, NotifyPublishEvent.AddAccountToChat);
            return new ServiceResponse<ChatBody>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = new ChatBody()
                {
                    Id = result.Id,
                    ImageUrl = result.Image,
                    Name = result.Name,
                    ParticipantIds = participants,
                    LastMessage = null,
                    IsSyncedReadStatus = false,
                    CountOfUnreadMessages = 0
                }
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
                Body = messages.Select(e => e.ToMessageBody())
            };
        }

        public async Task<ServiceResponse<IEnumerable<MessageBody>>> GetAllMessages(Guid accountId)
        {
            var messages = await _chatRepository.GetAllMessagesAsync(accountId);
            return new ServiceResponse<IEnumerable<MessageBody>>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = messages.Select(e => e.ToMessageBody())
            };
        }

        public async Task<ServiceResponse<MessageBody>> EditMessage(Guid accountId, MessageBody updatedMessage)
        {
            var message = await _chatRepository.UpdateMessage(accountId, updatedMessage);
            return new ServiceResponse<MessageBody>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = message.ToMessageBody()
            };
        }

        public async Task<ServiceResponse<MessageBody>> SendMessage(
            Guid senderId, Guid receiverId, string content)
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

            var message = await _chatRepository.AddMessageAsync(MessageType.Mail, content, chat.Chat, senderId, Guid.NewGuid());

            ChatLobby lobby = _chatConnectionService.GetConnections(chat.ChatId);

            if (!_chatConnectionService.LobbyIsExist(chat.ChatId))
            {
                var chatMemberships = await _chatRepository.GetChatSettingsAsync(chat.ChatId);
                var userIds = chatMemberships.Select(e => e.AccountId).ToList();
                lobby = _chatConnectionService.AddLobby(chat.ChatId, userIds);
            }

            await _chatConnector.SendMessage(lobby.ActiveSessions.Values, message.ToMessageBody(), WebSocketMessageType.Text, lobby.AllChatUsers, chat.Chat);

            _chatConnectionService.RemoveLobby(chat.ChatId);

            return new ServiceResponse<MessageBody>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = message.ToMessageBody()
            };
        }

        public async Task<ServiceResponse<MessageBody>> SendMessageToChat(
            Guid senderId, Guid chatId, string content)
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

            var message = await _chatRepository.AddMessageAsync(MessageType.Mail, content, chat.Chat, senderId, Guid.NewGuid());

            ChatLobby lobby = _chatConnectionService.GetConnections(chat.ChatId);

            if (!_chatConnectionService.LobbyIsExist(chat.ChatId))
            {
                var chatMemberships = await _chatRepository.GetChatSettingsAsync(chat.ChatId);
                var userIds = chatMemberships.Select(e => e.AccountId).ToList();
                lobby = _chatConnectionService.AddLobby(chat.ChatId, userIds);
            }

            await _chatConnector.SendMessage(lobby.ActiveSessions.Values, message.ToMessageBody(), WebSocketMessageType.Text, lobby.AllChatUsers, chat.Chat);

            _chatConnectionService.RemoveLobby(chat.ChatId);

            return new ServiceResponse<MessageBody>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = message.ToMessageBody()
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

        public async Task<ServiceResponse<bool>> CreateOrUpdateMessageDrafts(Guid accountId, List<MessageDraftBody> drafts)
        {
            foreach (var draft in drafts)
            {
                var membership = await _chatRepository.GetChatSettingsAsync(draft.ChatId, accountId);

                var result = await _chatRepository.CreateOrUpdateMessageDraft(membership, draft.Content);
            }

            return new ServiceResponse<bool>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = true
            };
        }

        public async Task<ServiceResponse<string>> GetMessageDraft(Guid accountId, Guid chatId)
        {
            var membership = await _chatRepository.GetChatSettingsAsync(chatId, accountId);

            return new ServiceResponse<string>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = membership.MessageDraft
            };
        }

        public async Task<ServiceResponse<NotificationSettings?>> SetEnabledNotifications(Guid accountId, Guid chatId, bool enable)
        {
            var membership = await _chatRepository.SetEnabledNotifications(accountId, chatId, enable);

            if (membership == null)
            {
                return new ServiceResponse<NotificationSettings?>
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    IsSuccess = true,
                    Errors = new[] { "��� ������� � ����" }
                };
            }

            return new ServiceResponse<NotificationSettings?>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = membership
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