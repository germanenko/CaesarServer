using System.Net.WebSockets;
using Planner_chat_server.Core.Entities.Models;
using Planner_chat_server.Core.Entities.Response;

namespace Planner_chat_server.Core.IService
{
    public interface IChatConnector
    {
        Task Invoke(
            ChatMembership chatMembership,
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