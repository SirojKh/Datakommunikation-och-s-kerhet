using System.Net.WebSockets;
using System.Text;

//Hantering av WebSocket-förfrågningar.
public class WebSocketMiddleware
{
    private readonly RequestDelegate _next;
    private readonly WebSocketManager _webSocketManager;
    private readonly MessageProcessor _messageProcessor;
    private readonly ILogger<WebSocketMiddleware> _logger;

    // Konstruktör för WebSocketMiddleware tar in nödvändiga tjänster och logger.
    public WebSocketMiddleware(RequestDelegate next, 
        WebSocketManager webSocketManager, 
        MessageProcessor messageProcessor,
        ILogger<WebSocketMiddleware> logger)
    {
        _next = next;
        _webSocketManager = webSocketManager;
        _messageProcessor = messageProcessor;
        _logger = logger;
    }

    // Denna metod kollas varje gång en förfrågan kommer in till servern.
    public async Task InvokeAsync(HttpContext context)
    {
        // Kontrollerar om förfrågan är en WebSocket-förfrågan.
        if (context.WebSockets.IsWebSocketRequest)
        {
            // Accepterar WebSocket-förfrågan.
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            _logger.LogInformation("WebSocket connection accepted.");
            
            // Lägger till den accepterade WebSocket till manager.
            _webSocketManager.AddSocket(webSocket);

            // Hanterar inkommande meddelanden för denna WebSocket.
            await HandleWebSocketCommunication(webSocket, context);
        }
        else
        {
            await _next(context);
        }
    }

    // Hanterar kommunikation för en given WebSocket.
    private async Task HandleWebSocketCommunication(WebSocket webSocket, HttpContext context)
    {
        var buffer = new byte[1024 * 4];
        try
        {
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                var incomingMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                _logger.LogInformation($"Received message: {incomingMessage}");
                await _messageProcessor.HandleMessageAsync(incomingMessage, webSocket);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
        }
        catch (WebSocketException ex)
        {
            _logger.LogError(ex, "WebSocket communication error.");
        }
        finally
        {
            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", CancellationToken.None);
            }
        }
    }
}
