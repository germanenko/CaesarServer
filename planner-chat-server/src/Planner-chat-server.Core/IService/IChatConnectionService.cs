using Planner_chat_server.Core.Entities.Response;

namespace Planner_chat_server.Core.IService
{
    public interface IChatConnectionService
    {
        ChatLobby? AddLobby(Guid chatId, List<Guid> allUserIds);
        bool LobbyIsExist(Guid chatId);
        ChatLobby? AddSessionToLobby(Guid chatId, ChatSession session);
        void RemoveConnection(Guid chatId, ChatSession session);
        ChatLobby? GetConnections(Guid chatId);
    }
}