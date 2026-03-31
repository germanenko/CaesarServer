using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using planner_chat_service.Core;
using planner_chat_service.Core.Entities.Models;
using planner_chat_service.Core.Entities.ValueObjects;
using planner_chat_service.Core.IRepository;
using planner_chat_service.Infrastructure.Data;
using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_common_package.Enums;

namespace planner_chat_service.Infrastructure.Repository
{
    public class ChatRepository : IChatRepository
    {
        private readonly ChatDbContext _context;
        private readonly ILogger<ChatRepository> _logger;

        public ChatRepository(ChatDbContext context, ILogger<ChatRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ChatMessage?> AddMessageAsync(MessageType messageType, string content, Chat chat, Guid senderId, Guid messageId, Guid? senderDeviceId)
        {
            var message = new ChatMessage
            {
                Id = messageId,
                Name = "Message",
                MessageType = messageType,
                Content = content,
                SenderId = senderId,
                ChatId = chat.Id,
                SenderDeviceId = senderDeviceId,
                Type = NodeType.Message
            };

            message = (await _context.ChatMessages.AddAsync(message))?.Entity;

            //await _context.NodeLinks.AddAsync(new NodeLink()
            //{
            //    ParentId = chat.Id,
            //    ChildId = message.Id,
            //    RelationType = RelationType.Contains
            //});

            await _context.SaveChangesAsync();

            return message;
        }

        public async Task<ChatBody?> AddPersonalChatAsync(Guid accountId, List<Guid> participants, CreateChatBody createChatBody, DateTime date)
        {
            if (participants.Count != 2 || (await GetPersonalChatAsync(participants[0], participants[1]) != null))
                return null;

            var chat = new Chat
            {
                Id = createChatBody.Id,
                Name = "Chat",
                ChatType = ChatType.Personal,
                Type = NodeType.Chat
            };

            var memberships = participants
                .Select(participantId => new ChatSettings
                {
                    AccountId = participantId,
                    Chat = chat,
                    DateLastViewing = date,
                })
                .ToList();

            chat.ChatMemberships = memberships;

            chat = (await _context.Chats.AddAsync(chat))?.Entity;

            var chatState = new ChatState()
            {
                Chat = chat
            };

            var chatUserState = new ChatUserState()
            {
                Chat = chat,
                AccountId = accountId
            };

            await _context.ChatStates.AddAsync(chatState);
            await _context.ChatUserStates.AddAsync(chatUserState);
            await _context.SaveChangesAsync();

            var chatBody = chat.ToNodeBody();
            chatBody.State = chatState.ToBody();
            chatBody.UserState = chatUserState.ToBody();

            return chatBody;
        }

        public async Task<Chat?> GetAsync(Guid id) => await _context.Chats.FindAsync(id);

        public async Task<List<ChatBody>> GetChatBodies(Guid accountId, Guid userSessionId, ChatType chatType)
        {
            var result = new List<ChatBody>();
            var type = chatType.ToString();

            var userChatMemberships = await _context.ChatSettings
                .Include(e => e.Chat)
                .Where(e => e.AccountId == accountId && e.Chat.ChatType == Enum.Parse<ChatType>(type))
                .ToListAsync();

            if (!userChatMemberships.Any())
                return result;

            var chatIds = userChatMemberships.Select(e => e.ChatId);

            var chatMemberships = await _context.ChatSettings
                .Where(e => chatIds.Contains(e.ChatId))
                .GroupBy(e => e.ChatId)
                .ToListAsync();

            foreach (var chatMembership in chatMemberships)
            {
                var chatId = chatMembership.Key;

                result.Add(await GetChat(accountId, userSessionId, chatId));

                //var userMembership = userChatMemberships.First(e => e.ChatId == chatId);
                //var userSession = await CreateOrGetAccountChatSessionAsync(userSessionId, userMembership.Id, userMembership.DateLastViewing);

                //var chat = userMembership.Chat;
                //var dateLastViewing = userSession.DateLastViewing;

                //var countOfUnreadMessages = await _context.NodeLinks
                //    .Join(_context.ChatMessages,
                //        n => n.ChildId,
                //        m => m.Id,
                //        (n, m) => new { Message = m, Link = n })
                //    .CountAsync(e => e.Link.ParentId == chatId && e.Message.SenderId != accountId && e.Message.HasBeenRead == false);

                //var lastMessage = await _context.ChatMessages
                //    .Join(_context.NodeLinks,
                //        m => m.Id,
                //        n => n.ChildId,
                //        (m, n) => new { Message = m, Link = n })
                //    .Where(x => x.Link.ParentId == chatId && x.Message.SentAt > dateLastViewing)
                //    .OrderByDescending(x => x.Message.SentAt)
                //    .Select(x => x.Message)
                //    .FirstOrDefaultAsync();

                //var chatBody = new ChatBody
                //{
                //    Id = chat.Id,
                //    Name = chat.Name,
                //    ImageUrl = chat.Image == null ? null : $"{Constants.WebUrlToChatIcon}/{chat.Image}",
                //    CountOfUnreadMessages = countOfUnreadMessages,
                //    IsSyncedReadStatus = userSession.DateLastViewing == userMembership.DateLastViewing,
                //    ParticipantIds = chatMembership.Where(x => x.AccountId != accountId).Select(e => e.AccountId).ToList(),
                //    Type = chat.ChatType,
                //    LastMessage = lastMessage?.ToMessageBody()
                //};

                //result.Add(chatBody);
            }

            return result;
        }

        public async Task<ChatBody> GetChat(Guid accountId, Guid userSessionId, Guid chatId)
        {
            var chatMembership = await _context.ChatSettings
                .Include(e => e.Chat)
                .Where(e => e.AccountId == accountId && e.ChatId == chatId)
                .FirstOrDefaultAsync();

            if (chatMembership == null)
            {
                return null;
            }

            var userSession = await CreateOrGetAccountChatSessionAsync(userSessionId, chatMembership.Id, chatMembership.DateLastViewing);

            var chat = chatMembership.Chat;
            var dateLastViewing = userSession.DateLastViewing;

            var countOfUnreadMessages = await _context.ChatMessages.CountAsync(e => e.ChatId == chatId && e.SenderId != accountId && e.HasBeenRead == false);

            var lastMessage = await _context.ChatMessages
                .Where(e => e.ChatId == chatId && e.SenderId != accountId && e.HasBeenRead == false)
                .OrderByDescending(x => x.SentAt)
                .FirstOrDefaultAsync();

            var state = await GetOrCreateChatState(chatId);
            var userState = await GetOrCreateChatUserState(chatId, accountId);

            var chatBody = new ChatBody
            {
                Id = chat.Id,
                Name = chat.Name,
                ChatType = chat.ChatType,
                ImageUrl = chat.Image == null ? null : $"{Constants.WebUrlToChatIcon}/{chat.Image}",
                CountOfUnreadMessages = countOfUnreadMessages,
                IsSyncedReadStatus = userSession.DateLastViewing == chatMembership.DateLastViewing,
                ParticipantIds = _context.ChatSettings
                .Where(x => x.ChatId == chatId && x.AccountId != accountId)
                .Select(e => e.AccountId)
                .ToList(),
                State = state.ToBody(),
                UserState = userState.ToBody()
            };

            return chatBody;
        }

        public async Task<ChatState> GetOrCreateChatState(Guid chatId)
        {
            var existingState = await _context.ChatStates.FirstOrDefaultAsync(x => x.ChatId == chatId);

            if (existingState != null)
            {
                var lastMessage = await _context.ChatMessages.OrderByDescending(x => x.SentAt).FirstOrDefaultAsync(x => x.ChatId == chatId);
                var lastEdit = await _context.ChatEdits.OrderByDescending(x => x.Seq).FirstOrDefaultAsync(x => x.ChatId == chatId);

                if (lastMessage != null)
                {
                    existingState.LastPreview = new MessagePreview()
                    {
                        MessageId = lastMessage.Id,
                        AuthorId = lastMessage.SenderId,
                        SentAt = lastMessage.SentAt,
                        Text = lastMessage.Content
                    };
                }

                existingState.EditCursor = lastEdit;
                existingState.LastMessageSeq = lastMessage.Seq;

                await _context.SaveChangesAsync();

                return existingState;
            }

            var state = new ChatState()
            {
                ChatId = chatId
            };

            state = (await _context.ChatStates.AddAsync(state)).Entity;

            await _context.SaveChangesAsync();

            return state;
        }

        public async Task<ChatUserState> GetOrCreateChatUserState(Guid chatId, Guid accountId)
        {
            var existingState = await _context.ChatUserStates.FirstOrDefaultAsync(x => x.ChatId == chatId && x.AccountId == accountId);

            if (existingState != null)
            {
                var lastMessage = await _context.ChatMessages.OrderByDescending(x => x.SentAt).FirstOrDefaultAsync(x => x.ChatId == chatId);

                if (lastMessage != null)
                {
                    if (lastMessage.Seq != existingState.LastReadSeq)
                    {
                        existingState.CachedUnreadCount = lastMessage.Seq - existingState.LastReadSeq;
                    }
                }

                await _context.SaveChangesAsync();

                return existingState;
            }

            var userState = new ChatUserState()
            {
                ChatId = chatId,
                AccountId = accountId
            };

            userState = (await _context.ChatUserStates.AddAsync(userState)).Entity;

            await _context.SaveChangesAsync();

            return userState;
        }

        public async Task<ChatEdit?> GetLastChatEdit(Guid chatId)
        {
            return await _context.ChatEdits
                .OrderByDescending(x => x.Seq)
                .FirstOrDefaultAsync(e => e.ChatId == chatId);
        }

        public async Task<List<ChatSettings>> GetChatSettingsAsync(Guid chatId)
        {
            return await _context.ChatSettings
                .Where(e => e.ChatId == chatId)
                .ToListAsync();
        }

        public async Task<List<ChatSettings>> GetChatSettingsByAccountIdAsync(Guid accountId)
        {
            return await _context.ChatSettings
                .Where(e => e.AccountId == accountId)
                .ToListAsync();
        }

        public async Task<ChatSettings?> GetChatSettingsAsync(Guid chatId, Guid accountId)
        {
            return await _context.ChatSettings
                .FirstOrDefaultAsync(e => e.ChatId == chatId && e.AccountId == accountId);
        }

        public async Task<ChatMessage?> GetMessageAsync(Guid id) => await _context.ChatMessages.FindAsync(id);

        public async Task<List<ChatMessage>> GetMessagesAsync(Guid chatId, int count, int countSkipped, bool isDescending = true)
        {
            var query = _context.ChatMessages
                .Where(e => e.ChatId == chatId);

            query = isDescending
                ? query.OrderByDescending(e => e.SentAt)
                : query.OrderBy(e => e.SentAt);

            return await query
                .Skip(countSkipped)
                .Take(count)
                .ToListAsync();
        }

        public async Task<ChatSettings?> GetPersonalChatAsync(Guid firstAccountId, Guid secondAccountId)
        {
            var query = _context.ChatSettings
                .Include(e => e.Chat)
                .Where(firstUser =>
                    firstUser.AccountId == firstAccountId &&
                    _context.ChatSettings.Any(secondUser =>
                        secondUser.AccountId == secondAccountId &&
                        secondUser.ChatId == firstUser.ChatId)
                );

            var result = await query.FirstOrDefaultAsync(e => e.Chat.ChatType == ChatType.Personal);
            return result;
        }

        public async Task<AccountChatSession> CreateOrGetAccountChatSessionAsync(Guid sessionId, Guid chatSettingsId, DateTime dateLastViewing)
        {
            var result = await _context.AccountChatSessions
                .FirstOrDefaultAsync(e => e.SessionId == sessionId && e.ChatSettingId == chatSettingsId);
            if (result != null)
                return result;

            result = new AccountChatSession
            {
                SessionId = sessionId,
                ChatSettingId = chatSettingsId,
                DateLastViewing = dateLastViewing
            };

            result = (await _context.AccountChatSessions.AddAsync(result)).Entity;
            await _context.SaveChangesAsync();

            return result;
        }

        public async Task<bool> UpdateLastViewingChatMembership(ChatSettings chatMembership, DateTime lastViewingDate)
        {
            if (chatMembership == null)
                return false;

            if (chatMembership.DateLastViewing > lastViewingDate)
                return false;

            chatMembership.DateLastViewing = lastViewingDate;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateLastViewingUserChatSession(AccountChatSession userChatSession, DateTime lastViewingDate)
        {
            if (userChatSession == null)
                return false;

            if (userChatSession.DateLastViewing > lastViewingDate)
                return false;

            userChatSession.DateLastViewing = lastViewingDate;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task CreateChatSettingsAsync(Guid taskId, List<Guid> accountIds)
        {
            if (accountIds == null || !accountIds.Any())
                return;

            var chat = await _context.Chats
                .FirstOrDefaultAsync(e => e.TaskId == taskId);

            if (chat == null)
                return;

            var uniqueAccountIds = accountIds.Distinct().ToList();

            var existingMemberships = await _context.ChatSettings
                .Where(e => e.ChatId == chat.Id &&
                            uniqueAccountIds.Contains(e.AccountId))
                .Select(e => e.AccountId)
                .ToListAsync();

            var newAccountIds = uniqueAccountIds
                .Except(existingMemberships)
                .ToList();

            if (!newAccountIds.Any())
                return;

            var newChatSettings = newAccountIds.Select(accountId => new ChatSettings
            {
                Id = Guid.NewGuid(),
                ChatId = chat.Id,
                AccountId = accountId,
                DateLastViewing = DateTime.UtcNow
            });

            var newNotificationSettings = newAccountIds.Select(accountId => new NotificationSettings
            {
                Id = Guid.NewGuid(),
                NodeId = chat.Id,
                AccountId = accountId
            });


            await _context.ChatSettings.AddRangeAsync(newChatSettings);
            await _context.SaveChangesAsync();
        }

        public async Task<ChatMessage?> UpdateMessage(Guid accountId, EditMessageBody updatedMessage)
        {
            var query = _context.ChatSettings
                .Include(e => e.Chat)
                .Where(user => user.AccountId == accountId);

            if (query.Count() == 0)
            {
                return null;
            }

            var message = await _context.ChatMessages
                .FirstOrDefaultAsync(m => m.Id == updatedMessage.MessageId);

            if (message == null)
            {
                return null;
            }

            message.Content = updatedMessage.Content;
            message.EditedAt = DateTime.UtcNow;

            if (message.ChatId != null)
                await InsertChatEditLog(message.ChatId.Value, message.Id, MessageAction.Edit);

            await _context.SaveChangesAsync();

            return message;
        }

        public async Task<ChatMessage?> DeleteMessage(Guid accountId, Guid messageId)
        {
            var query = _context.ChatSettings
                .Include(e => e.Chat)
                .Where(user => user.AccountId == accountId);

            if (query.Count() == 0)
            {
                return null;
            }

            var message = await _context.ChatMessages
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null)
            {
                return null;
            }

            message.IsDeleted = true;

            if (message.ChatId != null)
                await InsertChatEditLog(message.ChatId.Value, message.Id, MessageAction.Delete);

            await _context.SaveChangesAsync();

            return message;
        }

        public async Task<ChatMessage?> DeleteMessageForMe(Guid accountId, Guid messageId)
        {
            var query = _context.ChatSettings
                .Include(e => e.Chat)
                .Where(user => user.AccountId == accountId);

            if (query.Count() == 0)
            {
                return null;
            }

            var message = await _context.ChatMessages
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null)
            {
                return null;
            }

            var hiddenMessage = (await _context.UserHiddenMessages.AddAsync(new UserHiddenMessage(messageId, accountId))).Entity;

            if (hiddenMessage == null) return null;

            await _context.SaveChangesAsync();

            return message;
        }

        public async Task<ChatEdit> InsertChatEditLog(Guid chatId, Guid messageId, MessageAction messageAction)
        {
            var lastChatLog = await _context.ChatEdits.OrderByDescending(x => x.Seq).FirstOrDefaultAsync(x => x.MessageId == chatId);

            var log = new ChatEdit() { Action = messageAction, ChatId = lastChatLog?.ChatId ?? chatId, MessageId = messageId, Version = (lastChatLog?.Version ?? -1) + 1, EditedAt = DateTime.UtcNow };

            var edit = (await _context.AddAsync(log)).Entity;

            return edit;
        }

        public async Task<bool> CreateOrUpdateMessageDraft(ChatSettings chatSettings, string content)
        {
            var membership = await _context.ChatSettings
                .FirstOrDefaultAsync(m => m.Id == chatSettings.Id);

            if (membership == null)
                return false;

            if (membership.MessageDraft == content)
                return true;

            membership.MessageDraft = content;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}