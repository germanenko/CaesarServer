using System.Net.WebSockets;
using Planner_chat_server.Core.Entities.Request;
using Planner_chat_server.Core.Entities.Response;
using Planner_chat_server.Core.Enums;

namespace Planner_chat_server.Core.IService
{
    public interface IChatService
    {
        Task ConnectToChat(Guid accountId, Guid chatId, WebSocket socket, Guid sessionId);
        Task<ServiceResponse<IEnumerable<ChatBody>>> GetChats(Guid accountId, Guid sessionId, ChatType chatType);
        Task<ServiceResponse<Guid>> CreatePersonalChat(Guid accountId, Guid sessionId, CreateChatBody createChatBody, Guid addedAccountId);
        Task<ServiceResponse<IEnumerable<MessageBody>>> GetMessages(Guid accountId, Guid chatId, DynamicDataLoadingOptions options);
    }
}