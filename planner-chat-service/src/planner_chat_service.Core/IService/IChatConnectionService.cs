using planner_server_package.Entities;

namespace planner_chat_service.Core.IService
{
    public interface IChatConnectionService
    {
        ChatLobby? AddLobby(Guid chatId, List<Guid> allUserIds);
        bool LobbyIsExist(Guid chatId);
        ChatLobby? AddSessionToLobby(Guid chatId, ChatSession session);
        void RemoveLobby(Guid chatId);
        void RemoveConnection(Guid chatId, ChatSession session);
        ChatLobby? GetConnections(Guid chatId);
    }
}