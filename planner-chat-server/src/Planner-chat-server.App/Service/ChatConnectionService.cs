using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Planner_chat_server.Core.Entities.Response;
using Planner_chat_server.Core.IService;

namespace Planner_chat_server.App.Service
{
    public class ChatConnectionService : IChatConnectionService
    {
        private readonly ILogger<ChatConnectionService> _logger;
        private ConcurrentDictionary<Guid, ChatLobby> ChatLobbies { get; set; } = new();

        public ChatConnectionService(ILogger<ChatConnectionService> logger)
        {
            _logger = logger;
        }

        public ChatLobby? AddSessionToLobby(Guid chatId, ChatSession session)
        {
            if (ChatLobbies.TryGetValue(chatId, out var lobby) && !lobby.ActiveSessions.ContainsKey(session.SessionId))
            {
                lobby.ActiveSessions.TryAdd(session.SessionId, session);
                _logger.LogInformation($"connection is added {session.SessionId}");
                return lobby;
            }

            return null;
        }

        public ChatLobby? GetConnections(Guid chatId)
        {
            return ChatLobbies.TryGetValue(chatId, out var lobby) ? lobby : null;
        }

        public void RemoveConnection(Guid chatId, ChatSession session)
        {
            if (ChatLobbies.TryGetValue(chatId, out var chat))
            {
                if (chat.ActiveSessions.TryRemove(session.SessionId, out var _))
                {
                    if (!chat.ActiveSessions.Any())
                        ChatLobbies.Remove(chatId, out var _);
                }
            }
            _logger.LogInformation($"connection is deleted {session.SessionId}");
        }

        public ChatLobby? AddLobby(Guid chatId, List<Guid> allUserIds)
        {
            if (ChatLobbies.TryGetValue(chatId, out var _))
                return null;

            return ChatLobbies.GetOrAdd(chatId, new ChatLobby { AllChatUsers = allUserIds });
        }

        public bool LobbyIsExist(Guid chatId)
        {
            return ChatLobbies.ContainsKey(chatId);
        }
    }
}