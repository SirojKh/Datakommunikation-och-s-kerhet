using System.Net.WebSockets;
using System.Text.Json;
using Server.Models;

// Denna klass är ansvarig för att bearbeta inkommande WebSocket-meddelanden.
public class MessageProcessor
{
    private readonly ILogger<MessageProcessor> _logger;

    // Konstruktör tar in en logger.
    public MessageProcessor(ILogger<MessageProcessor> logger)
    {
        _logger = logger;
    }

    // Bearbetar ett inkommande meddelande baserat på dess innehåll.
    public async Task HandleMessageAsync(string jsonMessage, WebSocket socket)
    {
        try
        {
            var message = JsonSerializer.Deserialize<WebSocketMessage>(jsonMessage);
            switch (message.Type)
            {
                case MessageType.Chat:
                    var chatMessage = JsonSerializer.Deserialize<ChatMessage>(jsonMessage);
                    _logger.LogInformation($"Chat message received from {chatMessage.Sender}: {chatMessage.Content}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, $"Failed to deserialize message: {jsonMessage}");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            _logger.LogError(ex, "Unknown message type.");
        }
    }
}