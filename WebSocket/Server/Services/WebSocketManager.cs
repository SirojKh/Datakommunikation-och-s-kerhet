using System.Collections.Concurrent;
using System.Net.WebSockets;

// Denna klass hanterar förvaring och hantering av aktiva WebSockets.
public class WebSocketManager
{
    // En trådsäker samling som lagrar alla aktiva WebSockets.
    private readonly ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

    // Hämta en WebSocket baserat på dess unika ID.
    public WebSocket GetSocketById(string id)
    {
        return _sockets.FirstOrDefault(p => p.Key == id).Value;
    }

    public ConcurrentDictionary<string, WebSocket> GetAll()
    {
        return _sockets;
    }

    public string GetId(WebSocket socket)
    {
        return _sockets.FirstOrDefault(p => p.Value == socket).Key;
    }

    // Lägger till en ny WebSocket till samlingen.
    public void AddSocket(WebSocket socket)
    {
        var socketId = Guid.NewGuid().ToString();
        _sockets.TryAdd(socketId, socket);
    }

    // Ta bort en WebSocket från samlingen.
    public async Task RemoveSocket(string id)
    {
        WebSocket socket;
        _sockets.TryRemove(id, out socket);

        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Socket removed", CancellationToken.None);
    }
}