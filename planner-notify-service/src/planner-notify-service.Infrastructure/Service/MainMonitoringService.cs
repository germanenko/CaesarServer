using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using planner_notify_service.Core.Entities.Response;
using planner_notify_service.Core.IService;

namespace planner_notify_service.Infrastructure.Service
{
    public class MainMonitoringService : IMainMonitoringService
    {

        private ConcurrentDictionary<Guid, List<MainMonitoringSession>> _activeUserSessions { get; set; } = new();

        public MainMonitoringService()
        {

        }

        public MainMonitoringSession AddSession(Guid accountId, MainMonitoringSession session)
        {
            if (_activeUserSessions.TryGetValue(accountId, out var sessions))
            {
                var existingConnection = sessions.FirstOrDefault(e => e.SessionId == session.SessionId);
                if (existingConnection == null)
                    sessions.Add(session);

                return existingConnection ?? session;
            }

            _activeUserSessions.TryAdd(accountId, new List<MainMonitoringSession> { session });
            return session;
        }

        public IEnumerable<MainMonitoringSession> GetSessions(Guid accountId)
        {
            return _activeUserSessions.TryGetValue(accountId, out var sessions) ? sessions : new List<MainMonitoringSession>();
        }

        public bool RemoveSession(Guid accountId, Guid sessionId)
        {
            if (_activeUserSessions.TryGetValue(accountId, out var sessions))
            {
                var session = sessions.FirstOrDefault(e => e.SessionId == sessionId);
                if (session != null)
                    sessions.Remove(session);
                return true;
            }

            return false;
        }

        public async Task<IEnumerable<Guid>> SendMessageToSessions(Guid accountId, List<Guid> ignoredSessions, byte[] bytes)
        {
            var sessionsNotReceivedMessage = new List<Guid>();

            var userSessions = GetSessions(accountId);
            if (!userSessions.Any())
                return sessionsNotReceivedMessage;

            var sessions = userSessions.Where(e => !ignoredSessions.Contains(e.SessionId));

            foreach (var session in sessions)
            {
                var currentSession = sessions.FirstOrDefault(e => e.SessionId == session.SessionId);
                if (currentSession == null || !await SendMessage(currentSession.Socket, bytes, WebSocketMessageType.Text))
                    sessionsNotReceivedMessage.Add(session.SessionId);
            }

            return sessionsNotReceivedMessage;
        }

        public async Task SendMessageToAllSessions(Guid accountId, byte[] bytes)
        {
            var sessions = GetSessions(accountId);

            foreach (var session in sessions)
                await SendMessage(session.Socket, bytes, WebSocketMessageType.Text);
        }

        private async Task<bool> SendMessage(WebSocket socket, byte[] bytes, WebSocketMessageType messageType)
        {
            try
            {
                if (socket.State == WebSocketState.Open)
                    await socket.SendAsync(bytes, messageType, true, CancellationToken.None);
            }
            catch (Exception e) when (e is JsonException || e is WebSocketException)
            {
                return false;
            }

            return true;

        }
    }
}