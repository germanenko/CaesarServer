using planner_chat_service.Core.Entities.Models;
using planner_chat_service.Core.Entities.Request;
using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_common_package.Enums;
using planner_server_package;
using System.Net.WebSockets;

namespace planner_chat_service.Core.IService
{
    public interface IChatService
    {
        Task ConnectToChat(Guid accountId, Guid chatId, WebSocket socket, Guid sessionId);
        Task<ServiceResponse<IEnumerable<ChatBody>>> GetChats(Guid accountId, Guid sessionId, ChatType chatType);
        Task<ServiceResponse<ChatBody>> GetChat(Guid accountId, Guid userSessionId, Guid chatId);
        Task<ServiceResponse<ChatBody>> CreatePersonalChat(Guid accountId, Guid sessionId, CreateChatBody createChatBody);
        Task<ServiceResponse<IEnumerable<MessageBody>>> GetMessages(Guid accountId, Guid chatId, DynamicDataLoadingOptions options);
        Task<ServiceResponse<MessageBody>> EditMessage(Guid accountId, EditMessageBody updatedMessage);
        Task<ServiceResponse<MessageBody>> DeleteMessage(Guid accountId, Guid messageId);
        Task<ServiceResponse<MessageBody>> DeleteMessageForMe(Guid accountId, Guid messageId);
        Task<ServiceResponse<MessageBody>> SendMessage(Guid senderId, Guid? senderDeviceId, Guid receiverid, string content);
        Task<ServiceResponse<MessageBody>> SendMessageToChat(Guid senderId, Guid? senderDeviceId, Guid chatId, string content);
        Task<ServiceResponse<bool>> CreateOrUpdateMessageDraft(Guid accountId, Guid chatId, string content);
        Task<ServiceResponse<List<ChatSettings>>> GetChatsSettings(Guid accountId);
    }
}