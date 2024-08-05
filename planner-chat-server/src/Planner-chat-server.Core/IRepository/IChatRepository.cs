using Planner_chat_server.Core.Entities.Models;
using Planner_chat_server.Core.Entities.Response;
using Planner_chat_server.Core.Enums;

namespace Planner_chat_server.Core.IRepository
{
    public interface IChatRepository
    {
        Task<Chat?> GetAsync(Guid id);
        Task<Chat?> CreateTaskChatAsync(string name, Guid creatorId, Guid taskId);
        Task<ChatMembership?> AddMembershipAsync(Guid accountId, Chat chat);
        Task<List<ChatMembership>> GetChatMembershipsAsync(Guid chatId);
        Task<List<ChatMembership>> GetChatMembershipsByAccountIdAsync(Guid accountId);
        Task<ChatMembership?> GetMembershipAsync(Guid chatId, Guid accountId);
        Task<List<ChatMessage>> GetMessages(IEnumerable<Guid> messageIds);
        Task<ChatMessage?> AddMessageAsync(MessageType messageType, string content, Chat chat, Guid senderId);
        Task CreateAccountChatSessionAsync(IEnumerable<Guid> sessions, ChatMembership chatMembership, DateTime date);
        Task CreateAccountChatSessionsAsync(Guid session);
        Task<ChatMessage?> GetMessageAsync(Guid id);
        Task<Chat?> UpdateChatImage(Guid chatId, string filename);
        Task<AccountChatSession?> CreateOrGetAccountChatSessionAsync(Guid sessionId, Guid chatMembershipId, DateTime dateLastViewing);
        Task<IEnumerable<AccountChatSession>> GetAccountChatSessions(Guid chatMembershipId);
        Task<bool> UpdateLastViewingUserChatSession(AccountChatSession userChatSession, DateTime lastViewingDate);
        Task<List<ChatMessage>> GetMessagesAsync(Guid chatId, int count, int countSkipped, bool isDescending = true);
        Task<bool> UpdateLastViewingChatMembership(ChatMembership chatMembership, DateTime lastViewingDate);
        Task<ChatMembership?> CreateOrGetChatMembershipAsync(Chat chat, Guid accountId);
        Task CreateChatMembershipsAsync(Guid taskId, List<Guid> accountIds);
        Task<ChatMembership?> GetChatMembershipAsync(Guid chatId, Guid accountId);
        Task<List<ChatBody>> GetChatBodies(Guid accountId, Guid userSessionId, ChatType chatType);
        Task<ChatMembership?> GetPersonalChatAsync(Guid firstAccountId, Guid secondAccountId);
        Task<Chat?> AddPersonalChatAsync(List<Guid> participants, string name, DateTime date);
        Task<Chat?> GetByTaskIdAsync(Guid taskId);
    }
}