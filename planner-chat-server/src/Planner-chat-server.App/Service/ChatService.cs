using System.Net;
using System.Net.WebSockets;
using Planner_chat_server.Core.Entities.Events;
using Planner_chat_server.Core.Entities.Request;
using Planner_chat_server.Core.Entities.Response;
using Planner_chat_server.Core.Enums;
using Planner_chat_server.Core.IRepository;
using Planner_chat_server.Core.IService;

namespace Planner_chat_server.App.Service
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IChatConnectionService _chatConnectionService;
        private readonly IChatConnector _chatConnector;
        private readonly INotifyService _notifyService;

        public ChatService(
            IChatRepository chatRepository,
            IChatConnectionService chatConnectionService,
            IChatConnector chatConnector,
            INotifyService notifyService)
        {
            _chatRepository = chatRepository;
            _chatConnectionService = chatConnectionService;
            _chatConnector = chatConnector;
            _notifyService = notifyService;
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

            var chatMembership = await _chatRepository.GetChatMembershipAsync(chatId, accountId);
            if (chatMembership == null)
                return;

            var accountChatSession = await _chatRepository.CreateOrGetAccountChatSessionAsync(sessionId, chatMembership.Id, chatMembership.DateLastViewing);
            if (accountChatSession == null)
                return;

            if (!_chatConnectionService.LobbyIsExist(chatId))
            {
                var chatMemberships = await _chatRepository.GetChatMembershipsAsync(chatId);
                var userIds = chatMemberships.Select(e => e.AccountId).ToList();
                _chatConnectionService.AddLobby(chatId, userIds);
            }

            var lobby = _chatConnectionService.AddSessionToLobby(chatId, session);
            if (lobby == null)
                return;

            try
            {
                await _chatConnector.Invoke(chatMembership, chat, lobby, session, accountChatSession);
            }
            finally
            {
                _chatConnectionService.RemoveConnection(chatId, session);
            }
        }

        public async Task<ServiceResponse<Guid>> CreatePersonalChat(
            Guid accountId,
            Guid sessionId,
            CreateChatBody createChatBody,
            Guid addedAccountId)
        {
            var participants = new List<Guid>
            {
                accountId,
                addedAccountId
            };

            var currentDate = DateTime.UtcNow;
            var result = await _chatRepository.AddPersonalChatAsync(participants, createChatBody.Name, currentDate);
            if (result == null)
                return new ServiceResponse<Guid>
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

            var createChatEvent = new CreateChatEvent
            {
                ChatId = result.Id,
                Participants = chatMemberships
            };

            _notifyService.Publish(createChatEvent, NotifyPublishEvent.AddAccountToChat);
            return new ServiceResponse<Guid>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = result.Id
            };
        }

        public async Task<ServiceResponse<IEnumerable<ChatBody>>> GetChats(Guid accountId, Guid sessionId, ChatType chatType)
        {
            var chats = await _chatRepository.GetChatBodies(accountId, sessionId, chatType);
            return new ServiceResponse<IEnumerable<ChatBody>>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = chats
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
    }
}