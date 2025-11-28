using Microsoft.EntityFrameworkCore;
using Planner_chat_server.Core;
using Planner_chat_server.Core.Entities.Models;
using Planner_chat_server.Core.Entities.Request;
using Planner_chat_server.Core.Entities.Response;
using Planner_chat_server.Core.Enums;
using Planner_chat_server.Core.IRepository;
using Planner_chat_server.Infrastructure.Data;

namespace Planner_chat_server.Infrastructure.Repository
{
    public class ChatRepository : IChatRepository
    {
        private readonly ChatDbContext _context;

        public ChatRepository(ChatDbContext context)
        {
            _context = context;
        }

        public async Task<ChatMembership?> AddMembershipAsync(Guid accountId, Chat chat)
        {
            var chatMembership = await GetMembershipAsync(chat.Id, accountId);
            if (chatMembership != null)
                return null;

            chatMembership = new ChatMembership
            {
                AccountId = accountId,
                Chat = chat,
            };
            chatMembership = (await _context.ChatMemberships.AddAsync(chatMembership))?.Entity;
            await _context.SaveChangesAsync();

            return chatMembership;
        }

        public async Task<ChatMessage?> AddMessageAsync(MessageType messageType, string content, Chat chat, Guid senderId, Guid messageId)
        {
            var message = new ChatMessage
            {
                Id = messageId,
                Chat = chat,
                Type = messageType.ToString(),
                Content = content,
                SenderId = senderId,
            };

            message = (await _context.ChatMessages.AddAsync(message))?.Entity;
            await _context.SaveChangesAsync();

            return message;
        }

        public async Task<Chat?> AddPersonalChatAsync(List<Guid> participants, CreateChatBody createChatBody, DateTime date)
        {
            if (participants.Count != 2 || (await GetPersonalChatAsync(participants[0], participants[1]) != null))
                return null;

            var memberships = participants
                .Select(participantId => new ChatMembership
                {
                    AccountId = participantId,
                    DateLastViewing = date
                })
                .ToList();

            var chat = new Chat
            {
                Id = createChatBody.Id,
                Name = createChatBody.Name,
                Type = ChatType.Personal.ToString(),
                ChatMemberships = memberships,
            };

            chat = (await _context.Chats.AddAsync(chat))?.Entity;
            await _context.SaveChangesAsync();

            return chat;
        }

        public async Task<ChatMembership?> CreateOrGetChatMembershipAsync(Chat chat, Guid accountId)
        {
            var chatMembership = await GetMembershipAsync(chat.Id, accountId);
            if (chatMembership != null)
                return chatMembership;

            chatMembership = new ChatMembership
            {
                Chat = chat,
                AccountId = accountId
            };

            chatMembership = (await _context.ChatMemberships.AddAsync(chatMembership))?.Entity;
            await _context.SaveChangesAsync();
            return chatMembership;
        }

        public async Task CreateAccountChatSessionAsync(IEnumerable<Guid> sessions, ChatMembership chatMembership, DateTime date)
        {
            var existedChatSessions = await _context.AccountChatSessions.Where(e => e.ChatMembershipId == chatMembership.Id
                                                                                 && sessions.Contains(e.SessionId))
                                                                     .ToListAsync();

            var newSessions = sessions.Where(sessionId => !existedChatSessions.Any(e => e.SessionId == sessionId));
            var chatSessions = newSessions
                .Select(sessionId => new AccountChatSession
                {
                    SessionId = sessionId,
                    ChatMembership = chatMembership,
                    DateLastViewing = date
                })
                .ToList();

            await _context.AccountChatSessions.AddRangeAsync(chatSessions);
            await _context.SaveChangesAsync();
        }

        public async Task CreateAccountChatSessionsAsync(Guid sessionId)
        {
            var chatMemberships = await _context.ChatMemberships.Where(e => e.AccountId == sessionId)
                .ToListAsync();

            var userChatSessions = chatMemberships
                .Select(chatMembership => new AccountChatSession
                {
                    SessionId = sessionId,
                    ChatMembership = chatMembership,
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

            var userChatMemberships = await _context.ChatMemberships
                .Include(e => e.Chat)
                .Where(e => e.AccountId == accountId && e.Chat.Type == type)
                .ToListAsync();

            if (!userChatMemberships.Any())
                return result;

            var chatIds = userChatMemberships.Select(e => e.ChatId);

            var chatMemberships = await _context.ChatMemberships
                .Where(e => chatIds.Contains(e.ChatId))
                .GroupBy(e => e.ChatId)
                .ToListAsync();

            foreach (var chatMembership in chatMemberships)
            {
                var chatId = chatMembership.Key;
                var userMembership = userChatMemberships.First(e => e.ChatId == chatId);
                var userSession = await CreateOrGetAccountChatSessionAsync(userSessionId, userMembership.Id, userMembership.DateLastViewing);

                var chat = userMembership.Chat;
                var dateLastViewing = userSession.DateLastViewing;

                var countOfUnreadMessages = await _context.ChatMessages
                     .CountAsync(e => e.ChatId == chatId && e.SenderId != accountId && e.HasBeenRead == false);

                var lastMessage = await _context.ChatMessages
                        .Where(e => e.SentAt > dateLastViewing && e.ChatId == chatId)
                        .OrderByDescending(e => e.SentAt)
                        .FirstOrDefaultAsync();

                var chatBody = new ChatBody
                {
                    Id = chat.Id,
                    Name = chat.Name,
                    ImageUrl = chat.Image == null ? null : $"{Constants.WebUrlToChatIcon}/{chat.Image}",
                    CountOfUnreadMessages = countOfUnreadMessages,
                    IsSyncedReadStatus = userSession.DateLastViewing == userMembership.DateLastViewing,
                    ParticipantIds = chatMembership.Where(x => x.AccountId != accountId).Select(e => e.AccountId).ToList(),
                    Type = Enum.Parse<ChatType>(chat.Type),
                    LastMessage = lastMessage?.ToMessageBody()
                };

                result.Add(chatBody);
            }

            return result;
        }

        public async Task<ChatBody> GetChat(Guid accountId, Guid userSessionId, Guid chatId)
        {
            var chatMembership = await _context.ChatMemberships
                .Include(e => e.Chat)
                .Where(e => e.AccountId == accountId && e.ChatId == chatId)
                .FirstOrDefaultAsync();

            var userSession = await CreateOrGetAccountChatSessionAsync(userSessionId, chatMembership.Id, chatMembership.DateLastViewing);

            var chat = chatMembership.Chat;
            var dateLastViewing = userSession.DateLastViewing;

            var countOfUnreadMessages = await _context.ChatMessages
                .CountAsync(e => e.ChatId == chatId && e.SenderId != accountId && e.HasBeenRead == false);

            var lastMessage = await _context.ChatMessages
                    .Where(e => e.SentAt > dateLastViewing && e.ChatId == chatId)
                    .OrderByDescending(e => e.SentAt)
                    .FirstOrDefaultAsync();

            var chatBody = new ChatBody
            {
                Id = chat.Id,
                Name = chat.Name,
                ImageUrl = chat.Image == null ? null : $"{Constants.WebUrlToChatIcon}/{chat.Image}",
                CountOfUnreadMessages = countOfUnreadMessages,
                IsSyncedReadStatus = userSession.DateLastViewing == chatMembership.DateLastViewing,
                ParticipantIds = _context.ChatMemberships.Where(x => x.ChatId == chatId && x.AccountId != accountId).Select(e => e.AccountId).ToList(),
                LastMessage = lastMessage?.ToMessageBody()
            };

            return chatBody;
        }

        public async Task<List<ChatMembership>> GetChatMembershipsAsync(Guid chatId)
        {
            return await _context.ChatMemberships
                .Where(e => e.ChatId == chatId)
                .ToListAsync();
        }

        public async Task<List<ChatMembership>> GetChatMembershipsByAccountIdAsync(Guid accountId)
        {
            return await _context.ChatMemberships
                .Where(e => e.AccountId == accountId)
                .ToListAsync();
        }

        public async Task<ChatMembership?> GetMembershipAsync(Guid chatId, Guid accountId)
        {
            return await _context.ChatMemberships
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

        public async Task<List<ChatMessage>> GetAllMessagesAsync(Guid accountId)
        {
            var query = _context.ChatMemberships.Include(c => c.Chat).ThenInclude(x => x.Messages).Where(cm => cm.AccountId == accountId).Select(c => c.Chat);
            var messages = query.SelectMany(x => x.Messages.OrderByDescending(m => m.SentAt).Take(30));

            return await messages
                .ToListAsync();
        }

        public async Task<ChatMembership?> GetPersonalChatAsync(Guid firstAccountId, Guid secondAccountId)
        {
            var query = _context.ChatMemberships
                .Include(e => e.Chat)
                .Where(firstUser =>
                    firstUser.AccountId == firstAccountId &&
                    _context.ChatMemberships.Any(secondUser =>
                        secondUser.AccountId == secondAccountId &&
                        secondUser.ChatId == firstUser.ChatId)
                );

            var result = await query.FirstOrDefaultAsync(e => e.Chat.Type == ChatType.Personal.ToString());
            return result;
        }

        public async Task<AccountChatSession?> CreateOrGetAccountChatSessionAsync(Guid sessionId, Guid chatMembershipId, DateTime dateLastViewing)
        {
            var result = await _context.AccountChatSessions
                .FirstOrDefaultAsync(e => e.SessionId == sessionId && e.ChatMembershipId == chatMembershipId);
            if (result != null)
                return result;

            result = new AccountChatSession
            {
                SessionId = sessionId,
                ChatMembershipId = chatMembershipId,
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

        public async Task<bool> UpdateLastViewingChatMembership(ChatMembership chatMembership, DateTime lastViewingDate)
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
            return await _context.AccountChatSessions.Where(e => e.ChatMembershipId == chatMembershipId)
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
                Type = ChatType.Task.ToString(),
                TaskId = taskId,
                ChatMemberships = new List<ChatMembership>
                {
                    new ChatMembership { AccountId = creatorId }
                }
            };

            chat = (await _context.Chats.AddAsync(chat))?.Entity;
            await _context.SaveChangesAsync();

            return chat;
        }

        public Task<ChatMembership?> GetChatMembershipAsync(Guid chatId, Guid accountId)
        {
            return _context.ChatMemberships.FirstOrDefaultAsync(e => e.ChatId == chatId && e.AccountId == accountId);
        }

        public async Task CreateChatMembershipsAsync(Guid taskId, List<Guid> accountIds)
        {
            var chat = await _context.Chats.FirstOrDefaultAsync(e => e.TaskId == taskId);
            if (chat == null)
                return;

            var chatMemberships = await _context.ChatMemberships.Where(e => e.ChatId == chat.Id && accountIds.Contains(e.AccountId))
                .ToListAsync();

            var notExistedAccountIds = accountIds.Except(chatMemberships.Select(e => e.AccountId));
            var newChatMemberships = notExistedAccountIds.Select(accountId => new ChatMembership { ChatId = chat.Id, AccountId = accountId });
            await _context.ChatMemberships.AddRangeAsync(newChatMemberships);
            await _context.SaveChangesAsync();
        }

        public async Task<ChatMessage?> UpdateMessage(Guid accountId, MessageBody updatedMessage)
        {
            var query = _context.ChatMemberships
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
            message.UpdatedAt = updatedMessage.UpdatedAt;

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

        public async Task<bool> CreateOrUpdateMessageDraft(ChatMembership membership, string content)
        {
            var result = await _context.MessageDrafts
                .FirstOrDefaultAsync(e => e.MembershipId == membership.Id);

            if (result != null)
            {
                if(string.IsNullOrEmpty(content))
                {
                    _context.Remove(result);
                    await _context.SaveChangesAsync();
                    return true;
                }

                result.Content = content;

                await _context.SaveChangesAsync();

                return true;
            }

            result = new MessageDraft
            {
                Content = content,
                MembershipId = membership.Id,
                ChatMembership = membership
            };

            result = (await _context.MessageDrafts.AddAsync(result))?.Entity;
            await _context.SaveChangesAsync();

            return result != null;
        }

        public Task<MessageDraft?> GetMessageDraft(ChatMembership membership)
        {
            return _context.MessageDrafts.FirstOrDefaultAsync(e => e.MembershipId == membership.Id);
        }

        public async Task<List<MessageDraft>> GetMessageDrafts(Guid accountId)
        {
            return await _context.MessageDrafts.Include(d => d.ChatMembership).Where(x => x.ChatMembership.AccountId == accountId).ToListAsync();
        }
    }
}