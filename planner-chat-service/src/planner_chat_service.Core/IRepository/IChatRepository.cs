using planner_server_package.Entities;
using planner_server_package.Enums;
using planner_chat_service.Core.Entities.Models;

namespace planner_chat_service.Core.IRepository
{
    public interface IChatRepository
    {
        Task<Chat?> GetAsync(Guid id);
        Task<Chat?> CreateTaskChatAsync(string name, Guid creatorId, Guid taskId);
        Task<ChatSettings?> AddMembershipAsync(Guid accountId, Chat chat);
        Task<List<ChatSettings>> GetChatSettingsAsync(Guid chatId);
        Task<List<ChatSettings>> GetChatSettingsByAccountIdAsync(Guid accountId);
        Task<ChatSettings?> GetChatSettingsAsync(Guid chatId, Guid accountId);
        Task<List<ChatMessage>> GetMessages(IEnumerable<Guid> messageIds);
        Task<ChatMessage?> AddMessageAsync(MessageType messageType, string content, Chat chat, Guid senderId, Guid messageId);
        Task CreateAccountChatSessionAsync(IEnumerable<Guid> sessions, ChatSettings chatSettings, DateTime date);
        Task CreateAccountChatSessionsAsync(Guid session);
        Task<ChatMessage?> GetMessageAsync(Guid id);
        Task<Chat?> UpdateChatImage(Guid chatId, string filename);
        Task<AccountChatSession?> CreateOrGetAccountChatSessionAsync(Guid sessionId, Guid chatSettingsId, DateTime dateLastViewing);
        Task<IEnumerable<AccountChatSession>> GetAccountChatSessions(Guid chatAccessId);
        Task<bool> UpdateLastViewingUserChatSession(AccountChatSession userChatSession, DateTime lastViewingDate);
        Task<List<ChatMessage>> GetMessagesAsync(Guid chatId, int count, int countSkipped, bool isDescending = true);
        Task<List<ChatMessage>> GetAllMessagesAsync(Guid accountId);
        Task<bool> UpdateLastViewingChatMembership(ChatSettings chatSetttings, DateTime lastViewingDate);
        Task<ChatSettings?> CreateOrGetChatSettingsAsync(Chat chat, Guid accountId);
        Task CreateChatSettingsAsync(Guid taskId, List<Guid> accountIds);
        Task<List<ChatBody>> GetChatBodies(Guid accountId, Guid userSessionId, ChatType chatType);
        Task<ChatBody> GetChat(Guid accountId, Guid userSessionId, Guid chatId);
        Task<ChatSettings?> GetPersonalChatAsync(Guid firstAccountId, Guid secondAccountId);
        Task<Chat?> AddPersonalChatAsync(List<Guid> participants, ChatBody createChatBody, DateTime date);
        Task<Chat?> GetByTaskIdAsync(Guid taskId);
        Task<ChatMessage?> UpdateMessage(Guid accountId, MessageBody message);
        Task<ChatMessage?> SetMessageIsRead(ChatMessage message);
        Task<bool> CreateOrUpdateMessageDraft(ChatSettings chatSettings, string content);
        Task<NotificationSettings?> SetEnabledNotifications(Guid accountId, Guid chatId, bool enable);
        Task<bool> NotificationsIsEnabled(Guid accountId, Guid chatId);
        Task<List<Guid>> GetUsersWithEnabledNotifications(IEnumerable<Guid> accountIds, Guid chatId);
        Task<AccessRight?> GetChatAccess(Guid accountId, Guid chatId);
    }
}