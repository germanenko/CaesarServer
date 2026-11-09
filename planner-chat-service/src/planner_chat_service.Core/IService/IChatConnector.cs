using planner_chat_service.Core.Entities.Models;
using planner_client_package.Entities;
using planner_common_package.Enums;
using System.Net.WebSockets;

namespace planner_chat_service.Core.IService
{
    public interface IChatConnector
    {
        Task Invoke(
            Guid accountId,
            Chat chat,
            ChatLobby lobby,
            ChatSession currentSession,
            AccountChatSession userChatSession);
        Task SendMessage(
            IEnumerable<ChatSession> connections,
            MessageBody message,
            WebSocketMessageType messageType,
            IEnumerable<Guid> accountIds,
            Guid chatId);

        Task<IEnumerable<AccountSessions>> SendMessageToConnectedUsers(
            IEnumerable<ChatSession> sessions,
            object messageBody,
            WebSocketMessageKind messageKind,
            WebSocketMessageType messageType);
    }
}