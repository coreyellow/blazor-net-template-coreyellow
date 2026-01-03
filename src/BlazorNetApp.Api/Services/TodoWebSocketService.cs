using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace BlazorNetApp.Api.Services;

public class TodoWebSocketService
{
    private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();
    private readonly ILogger<TodoWebSocketService> _logger;

    public TodoWebSocketService(ILogger<TodoWebSocketService> logger)
    {
        _logger = logger;
    }

    public async Task HandleWebSocketAsync(WebSocket webSocket, string connectionId)
    {
        _sockets.TryAdd(connectionId, webSocket);
        _logger.LogInformation("WebSocket client connected: {ConnectionId}", connectionId);

        var buffer = new byte[1024 * 4];
        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in WebSocket connection {ConnectionId}", connectionId);
        }
        finally
        {
            _sockets.TryRemove(connectionId, out _);
            _logger.LogInformation("WebSocket client disconnected: {ConnectionId}", connectionId);
        }
    }

    public async Task BroadcastTodoChangeAsync(string action, object data)
    {
        if (_sockets.IsEmpty)
        {
            return;
        }

        var message = new
        {
            action = action,
            data = data,
            timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(message);
        var bytes = Encoding.UTF8.GetBytes(json);
        var arraySegment = new ArraySegment<byte>(bytes);

        var disconnectedSockets = new List<string>();

        foreach (var socket in _sockets)
        {
            if (socket.Value.State == WebSocketState.Open)
            {
                try
                {
                    await socket.Value.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send message to WebSocket client {ConnectionId}", socket.Key);
                    disconnectedSockets.Add(socket.Key);
                }
            }
            else
            {
                disconnectedSockets.Add(socket.Key);
            }
        }

        // Clean up disconnected sockets
        foreach (var socketId in disconnectedSockets)
        {
            _sockets.TryRemove(socketId, out _);
        }

        _logger.LogInformation("Broadcasted {Action} to {Count} WebSocket clients", action, _sockets.Count);
    }

    public int ConnectedClientsCount => _sockets.Count;
}
