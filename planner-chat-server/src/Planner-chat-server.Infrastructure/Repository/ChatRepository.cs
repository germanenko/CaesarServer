using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Planner_chat_server.Core;
using Planner_chat_server.Core.Entities.Models;
using Planner_chat_server.Core.Entities.Request;
using Planner_chat_server.Core.Entities.Response;
using Planner_chat_server.Core.Enums;
using Planner_chat_server.Core.IRepository;
using Planner_chat_server.Infrastructure.Data;
using System.Linq;

namespace Planner_chat_server.Infrastructure.Repository
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

        public async Task<ChatSettings?> AddMembershipAsync(Guid accountId, Chat chat)
        {
            var chatMembership = await GetChatSettingsAsync(chat.Id, accountId);
            if (chatMembership != null)
                return null;

            chatMembership = new ChatSettings
            {
                AccountId = accountId,
                Chat = chat,
            };
            chatMembership = (await _context.ChatSettings.AddAsync(chatMembership))?.Entity;

            await _context.AccessRights.AddAsync(new AccessRight
            {
                AccountId = accountId,
                NodeId = chat.Id,
                NodeType = NodeType.Message
            });

            await _context.SaveChangesAsync();

            return chatMembership;
        }

        public async Task<ChatMessage?> AddMessageAsync(MessageType messageType, string content, Chat chat, Guid senderId, Guid messageId)
        {
            var message = new ChatMessage
            {
                Id = messageId,
                Name = "Message",
                MessageType = messageType,
                Content = content,
                SenderId = senderId,
            };

            await _context.NodeLinks.AddAsync(new NodeLink()
            {
                ParentNode = chat,
                ChildNode = message,
                RelationType = RelationType.Contains
            });

            message = (await _context.ChatMessages.AddAsync(message))?.Entity;
            await _context.SaveChangesAsync();

            return message;
        }

        public async Task<Chat?> AddPersonalChatAsync(List<Guid> participants, CreateChatBody createChatBody, DateTime date)
        {
            if (participants.Count != 2 || (await GetPersonalChatAsync(participants[0], participants[1]) != null))
                return null;

            var chat = new Chat
            {
                Id = createChatBody.Id,
                Name = createChatBody.Name,
                ChatType = ChatType.Personal
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

            var access = participants
                .Select(participantId => new AccessRight
                {
                    AccountId = participantId,
                    Node = chat
                })
                .ToList();

            await _context.AccessRights.AddRangeAsync(access);

            chat = (await _context.Chats.AddAsync(chat))?.Entity;
            await _context.SaveChangesAsync();

            return chat;
        }

        public async Task<ChatSettings?> CreateOrGetChatSettingsAsync(Chat chat, Guid accountId)
        {
            var chatMembership = await GetChatSettingsAsync(chat.Id, accountId);
            if (chatMembership != null)
                return chatMembership;

            chatMembership = new ChatSettings
            {
                Chat = chat,
                AccountId = accountId
            };

            chatMembership = (await _context.ChatSettings.AddAsync(chatMembership))?.Entity;
            await _context.SaveChangesAsync();
            return chatMembership;
        }

        public async Task CreateAccountChatSessionAsync(IEnumerable<Guid> sessions, ChatSettings chatMembership, DateTime date)
        {
            var existedChatSessions = await _context.AccountChatSessions.Where(e => e.ChatSettingId == chatMembership.Id
                                                                                 && sessions.Contains(e.SessionId))
                                                                     .ToListAsync();

            var newSessions = sessions.Where(sessionId => !existedChatSessions.Any(e => e.SessionId == sessionId));
            var chatSessions = newSessions
                .Select(sessionId => new AccountChatSession
                {
                    SessionId = sessionId,
                    ChatSetting = chatMembership,
                    DateLastViewing = date
                })
                .ToList();

            await _context.AccountChatSessions.AddRangeAsync(chatSessions);
            await _context.SaveChangesAsync();
        }

        public async Task CreateAccountChatSessionsAsync(Guid sessionId)
        {
            var chatMemberships = await _context.ChatSettings.Where(e => e.AccountId == sessionId)
                .ToListAsync();

            var userChatSessions = chatMemberships
                .Select(chatMembership => new AccountChatSession
                {
                    SessionId = sessionId,
                    ChatSetting = chatMembership,
                    DateLastViewing = chatMembership.DateLastViewing
                });

            await _context.AccountChatSessions.AddRangeAsync(userChatSessions);
            await _context.SaveChangesAsync();
        }

        public async Task<Chat?> GetAsync(Guid id) => await _context.Chats.FindAsync(id);

        public async Task<Chat?> GetByTaskIdAsync(Guid taskId) => await _context.Chats.FirstOrDefaultAsync(e => e.TaskId == taskId);

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

            var userSession = await CreateOrGetAccountChatSessionAsync(userSessionId, chatMembership.Id, chatMembership.DateLastViewing);

            var chat = chatMembership.Chat;
            var dateLastViewing = userSession.DateLastViewing;

            var countOfUnreadMessages = await _context.NodeLinks
                .Join(_context.ChatMessages,
                    n => n.ChildId,
                    m => m.Id,
                    (n, m) => new { Message = m, Link = n })
                .CountAsync(e => e.Link.ParentId == chatId && e.Message.SenderId != accountId && e.Message.HasBeenRead == false);

            var lastMessage = await _context.ChatMessages
                .Join(_context.NodeLinks,
                    m => m.Id,
                    n => n.ChildId,
                    (m, n) => new { Message = m, Link = n })
                .Where(x => x.Link.ParentId == chatId && x.Message.SentAt > dateLastViewing)
                .OrderByDescending(x => x.Message.SentAt)
                .Select(x => x.Message)
                .FirstOrDefaultAsync();

            var chatBody = new ChatBody
            {
                Id = chat.Id,
                Name = chat.Name,
                Type = chat.ChatType,
                ImageUrl = chat.Image == null ? null : $"{Constants.WebUrlToChatIcon}/{chat.Image}",
                CountOfUnreadMessages = countOfUnreadMessages,
                IsSyncedReadStatus = userSession.DateLastViewing == chatMembership.DateLastViewing,
                ParticipantIds = _context.ChatSettings
                .Where(x => x.ChatId == chatId && x.AccountId != accountId)
                .Select(e => e.AccountId)
                .ToList(),
                LastMessage = lastMessage?.ToMessageBody()
            };

            return chatBody;
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

        public async Task<List<ChatMessage>> GetMessages(IEnumerable<Guid> messageIds)
        {
            return await _context.ChatMessages
                .Where(e => messageIds.Contains(e.Id))
                .ToListAsync();
        }

        public async Task<List<ChatMessage>> GetMessagesAsync(Guid chatId, int count, int countSkipped, bool isDescending = true)
        {
            var query = _context.NodeLinks
                .Where(e => e.ParentId == chatId)
                .Join(_context.ChatMessages,
                n => n.ChildId,
                m => m.Id,
                (n, m) => m);

            query = isDescending
                ? query.OrderByDescending(e => e.SentAt)
                : query.OrderBy(e => e.SentAt);

            return await query
                .Skip(countSkipped)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<ChatMessage>> GetAllMessagesAsync(Guid accountId)
        {
            var query = _context.AccessRights.Include(c => c.Node).Where(cm => cm.AccountId == accountId).Select(c => c.Node as Chat);
            var messages = query.SelectMany(x => x.Messages.OrderByDescending(m => m.SentAt).Take(30));

            return await messages
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

        public async Task<AccountChatSession?> CreateOrGetAccountChatSessionAsync(Guid sessionId, Guid chatMembershipId, DateTime dateLastViewing)
        {
            var result = await _context.AccountChatSessions
                .FirstOrDefaultAsync(e => e.SessionId == sessionId && e.ChatSettingId == chatMembershipId);
            if (result != null)
                return result;

            result = new AccountChatSession
            {
                SessionId = sessionId,
                ChatSettingId = chatMembershipId,
                DateLastViewing = dateLastViewing
            };

            result = (await _context.AccountChatSessions.AddAsync(result))?.Entity;
            await _context.SaveChangesAsync();

            return result;
        }

        public async Task<Chat?> UpdateChatImage(Guid chatId, string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return null;

            var chat = await GetAsync(chatId);
            if (chat == null)
                return null;

            chat.Image = filename;
            await _context.SaveChangesAsync();

            return chat;
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

        public async Task<IEnumerable<AccountChatSession>> GetAccountChatSessions(Guid chatMembershipId)
        {
            return await _context.AccountChatSessions.Where(e => e.ChatSettingId == chatMembershipId)
                .ToListAsync();
        }

        public async Task<Chat?> CreateTaskChatAsync(string name, Guid creatorId, Guid taskId)
        {
            var result = await _context.Chats.FirstOrDefaultAsync(e => e.TaskId == taskId);
            if (result != null)
                return null;

            var chat = new Chat
            {
                Name = name,
                ChatType = ChatType.Task,
                TaskId = taskId,
                ChatMemberships = new List<ChatSettings>
                {
                    new ChatSettings { AccountId = creatorId }
                }
            };

            chat = (await _context.Chats.AddAsync(chat))?.Entity;
            await _context.SaveChangesAsync();

            return chat;
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

            var newChatMemberships = newAccountIds.Select(accountId => new ChatSettings
            {
                Id = Guid.NewGuid(),
                ChatId = chat.Id,
                AccountId = accountId,
                DateLastViewing = DateTime.UtcNow,
                NotificationsEnabled = true
            });

            await _context.ChatSettings.AddRangeAsync(newChatMemberships);
            await _context.SaveChangesAsync();
        }

        public async Task<ChatMessage?> UpdateMessage(Guid accountId, MessageBody updatedMessage)
        {
            var query = _context.ChatSettings
                .Include(e => e.Chat)
                .Where(user => user.AccountId == accountId);

            if(query.Count() == 0)
            {
                return null;
            }

            var message = _context.ChatMessages
                .Where(m => m.Id == updatedMessage.Id).FirstOrDefault();

            if(message == null)
            {
                return null;
            }

            message.Content = updatedMessage.Content;

            _context.History.Add(new History()
            {
                NodeId = message.Id,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = accountId,
            });

            await _context.SaveChangesAsync();

            return message;
        }

        public async Task<ChatMessage?> SetMessageIsRead(ChatMessage readMessage)
        {
            var message = _context.ChatMessages
                .Where(m => m.Id == readMessage.Id).FirstOrDefault();

            if (message == null)
            {
                return null;
            }

            message.HasBeenRead = true;

            await _context.SaveChangesAsync();

            return message;
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

        public async Task<bool> NotificationsIsEnabled(Guid accountId, Guid chatId)
        {
            var membership = await _context.ChatSettings.FirstOrDefaultAsync(x => x.AccountId == accountId && x.ChatId == chatId);

            return membership.NotificationsEnabled;
        }

        public async Task<List<Guid>> GetUsersWithEnabledNotifications(IEnumerable<Guid> accountIds, Guid chatId)
        {
            return await _context.ChatSettings
                .Where(cm => accountIds.Contains(cm.AccountId) &&
                            cm.ChatId == chatId &&
                            cm.NotificationsEnabled)
                .Select(cm => cm.AccountId)
                .ToListAsync();
        }

        public async Task<ChatSettings?> SetEnabledNotifications(Guid accountId, Guid chatId, bool enable)
        {
            var membership = await _context.ChatSettings.FirstOrDefaultAsync(x => x.AccountId == accountId && x.ChatId == chatId);

            if(membership == null)
            {
                return null;
            }

            membership.NotificationsEnabled = enable;

            await _context.SaveChangesAsync();

            return membership;
        }

        public async Task<AccessRight?> GetChatAccess(Guid accountId, Guid chatId)
        {
            var acc = await _context.AccessRights.ToListAsync();
            foreach (var ac in acc)
            {
                _logger.LogInformation($"{ac.AccountId}-{ac.NodeId}");
            }
            return await _context.AccessRights.FirstOrDefaultAsync(cm => cm.AccountId == accountId && cm.NodeId == chatId);
        }
    }
}