using planner_chat_service.Core.Entities.Models;
using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_common_package.Enums;

namespace planner_chat_service.Core.IRepository
{
    public interface IChatRepository
    {
        Task<Chat?> GetAsync(Guid id);
        Task<List<ChatSettings>> GetChatSettingsAsync(Guid chatId);
        Task<List<ChatSettings>> GetChatSettingsByAccountIdAsync(Guid accountId);
        Task<ChatSettings?> GetChatSettingsAsync(Guid chatId, Guid accountId);
        Task<ChatMessage?> AddMessageAsync(MessageType messageType, string content, Chat chat, Guid senderId, Guid messageId, Guid? senderDeviceId);
        Task<ChatMessage?> GetMessageAsync(Guid id);
        Task<AccountChatSession> CreateOrGetAccountChatSessionAsync(Guid sessionId, Guid chatSettingsId, DateTime dateLastViewing);
        Task<bool> UpdateLastViewingUserChatSession(AccountChatSession userChatSession, DateTime lastViewingDate);
        Task<List<ChatMessage>> GetMessagesAsync(Guid chatId, int count, int countSkipped, bool isDescending = true);
        Task<bool> UpdateLastViewingChatMembership(ChatSettings chatSetttings, DateTime lastViewingDate);
        Task CreateChatSettingsAsync(Guid taskId, List<Guid> accountIds);
        Task<List<ChatBody>> GetChatBodies(Guid accountId, Guid userSessionId, ChatType chatType);
        Task<ChatBody> GetChat(Guid accountId, Guid userSessionId, Guid chatId);
        Task<ChatState> GetOrCreateChatState(Guid chatId);
        Task<ChatUserState> GetOrCreateChatUserState(Guid accountId, Guid chatId);
        Task<ChatEdit?> GetLastChatEdit(Guid chatId);
        Task<ChatSettings?> GetPersonalChatAsync(Guid firstAccountId, Guid secondAccountId);
        Task<ChatBody?> AddPersonalChatAsync(Guid accountId, List<Guid> participants, CreateChatBody createChatBody, DateTime date);
        Task<ChatMessage?> UpdateMessage(Guid accountId, EditMessageBody message);
        Task<ChatMessage?> DeleteMessage(Guid accountId, Guid messageId);
        Task<ChatMessage?> DeleteMessageForMe(Guid accountId, Guid messageId);
        Task<bool> CreateOrUpdateMessageDraft(ChatSettings chatSettings, string content);
    }
}