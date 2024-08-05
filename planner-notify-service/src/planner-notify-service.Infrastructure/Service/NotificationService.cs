using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using planner_notify_service.Core.Entities.Response;
using planner_notify_service.Core.IService;

namespace planner_notify_service.Infrastructure.Service
{
    public class NotificationService : INotificationService
    {
        private ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, NotificationSession>> ActiveSessions { get; set; } = new();

        public NotificationService()
        {

        }

        public NotificationSession AddSession(Guid accountId, NotificationSession session)
        {
            if (ActiveSessions.TryGetValue(accountId, out var sessions))
            {
                sessions.TryGetValue(session.SessionId, out var existingSession);
                if (existingSession == null)
                {
                    sessions.TryAdd(session.SessionId, session);
                }

                return existingSession ?? session;
            }

            var newSessions = new ConcurrentDictionary<Guid, NotificationSession>();
            newSessions.TryAdd(session.SessionId, session);

            ActiveSessions.TryAdd(accountId, newSessions);
            return session;
        }

        public IEnumerable<NotificationSession> GetSessions(Guid accountId)
        {
            var sessions = ActiveSessions.GetValueOrDefault(accountId);
            return sessions?.Values ?? new List<NotificationSession>();
        }

        public bool RemoveSession(Guid accountId, Guid sessionId)
        {
            if (ActiveSessions.TryGetValue(accountId, out var sessions))
            {
                sessions.TryRemove(sessionId, out var _);
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

        public async Task SendMessageToSessions(Guid accountId, byte[] bytes)
        {
            var sessions = GetSessions(accountId);

            foreach (var session in sessions)
                await SendMessage(session.Socket, bytes, WebSocketMessageType.Text);
        }
        private async Task<bool> SendMessage(
            WebSocket socket,
            byte[] bytes,
            WebSocketMessageType messageType)
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