using System.Net.WebSockets;
using CaesarServerLibrary.Entities;
using Planner_chat_server.Core.Entities.Models;

namespace Planner_chat_server.Core.IService
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