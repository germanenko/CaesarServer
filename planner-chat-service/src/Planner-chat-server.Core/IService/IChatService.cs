using CaesarServerLibrary.Entities;
using CaesarServerLibrary.Enums;
using Planner_chat_server.Core.Entities.Models;
using Planner_chat_server.Core.Entities.Request;
using System.Net.WebSockets;

namespace Planner_chat_server.Core.IService
{
    public interface IChatService
    {
        Task ConnectToChat(Guid accountId, Guid chatId, WebSocket socket, Guid sessionId);
        Task<ServiceResponse<IEnumerable<ChatBody>>> GetChats(Guid accountId, Guid sessionId, ChatType chatType);
        Task<ServiceResponse<ChatBody>> GetChat(Guid accountId, Guid userSessionId, Guid chatId);
        Task<ServiceResponse<ChatBody>> CreatePersonalChat(Guid accountId, Guid sessionId, ChatBody createChatBody, Guid addedAccountId);
        Task<ServiceResponse<IEnumerable<MessageBody>>> GetMessages(Guid accountId, Guid chatId, DynamicDataLoadingOptions options);
        Task<ServiceResponse<IEnumerable<MessageBody>>> GetAllMessages(Guid accountId);
        Task<ServiceResponse<MessageBody>> EditMessage(Guid accountId, MessageBody updatedMessage);
        Task<ServiceResponse<MessageBody>> SendMessage(Guid senderId, Guid receiverid, string content);
        Task<ServiceResponse<MessageBody>> SendMessageToChat(Guid senderId, Guid chatId, string content);
        Task<ServiceResponse<bool>> CreateOrUpdateMessageDraft(Guid accountId, Guid chatId, string content);
        Task<ServiceResponse<bool>> CreateOrUpdateMessageDrafts(Guid accountId, List<MessageDraftBody> drafts);
        Task<ServiceResponse<string>> GetMessageDraft(Guid accountId, Guid chatId);
        Task<ServiceResponse<NotificationSettings?>> SetEnabledNotifications(Guid accountId, Guid chatId, bool enable);
        Task<ServiceResponse<List<ChatSettings>>> GetChatsSettings(Guid accountId);
    }
}