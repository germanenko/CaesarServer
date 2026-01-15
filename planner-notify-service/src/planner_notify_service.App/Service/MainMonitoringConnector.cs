using System.Net.WebSockets;
using planner_notify_service.Core.Entities.Response;
using planner_notify_service.Core.IService;

namespace planner_notify_service.App.Service
{
    public class MainMonitoringConnector : IMainMonitoringConnector
    {
        private readonly IMainMonitoringService _mainMonitoringService;

        public MainMonitoringConnector(IMainMonitoringService mainMonitoringService)
        {
            _mainMonitoringService = mainMonitoringService;
        }

        public async Task ConnectToMainMonitoringService(Guid accountId, Guid sessionId, WebSocket socket)
        {
            var mainMonitoring = new MainMonitoringSession
            {
                Socket = socket,
                SessionId = sessionId
            };

            _mainMonitoringService.AddSession(accountId, mainMonitoring);

            await Loop(socket);

            _mainMonitoringService.RemoveSession(accountId, sessionId);
        }

        private async Task Loop(WebSocket ws)
        {
            try
            {
                while (ws.State == WebSocketState.Open)
                {
                    var stream = await ReceiveMessage(ws, CancellationToken.None);
                    if (stream == null || stream.Length == 0)
                        return;
                }
            }
            catch (WebSocketException e)
            {
            }
            finally
            {
                if (ws.State == WebSocketState.Open)
                    await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
            }
        }

        private async Task<MemoryStream?> ReceiveMessage(WebSocket webSocket, CancellationToken token)
        {
            byte[] bytes = new byte[4096];
            MemoryStream stream = new();

            WebSocketReceiveResult? receiveResult;
            do
            {
                receiveResult = await webSocket.ReceiveAsync(bytes, token);
                if (receiveResult.MessageType == WebSocketMessageType.Close && webSocket.State != WebSocketState.Closed)
                {
                    await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, token);
                    return null;
                }
                else if (receiveResult.Count > 0)
                    stream.Write(bytes, 0, receiveResult.Count);
            } while (!receiveResult.EndOfMessage && webSocket.State == WebSocketState.Open);

            return stream;
        }
    }
}