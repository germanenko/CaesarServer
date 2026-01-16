using System.Net.WebSockets;
using planner_server_package.Entities;
using planner_chat_service.Core.Entities.Models;

namespace planner_chat_service.Core.IService
{
    public interface IChatConnector
    {
        Task Invoke(
            AccessRight accessRight,
            Chat chat,
            ChatLobby lobby,
            ChatSession currentSession,
            AccountChatSession userChatSession);
        Task SendMessage(
            IEnumerable<ChatSession> connections,
            MessageBody message,
            WebSocketMessageType messageType,
            IEnumerable<Guid> accountIds,
            Chat chat);
    }
}