namespace Server.Models;

// Denna uppräkning listar alla typer av meddelanden som kan skickas över WebSocket.
public enum MessageType
{
    Chat
}

// Grundläggande klass för ett WebSocket-meddelande. Alla andra meddelandetyper bör ärva från denna.
public class WebSocketMessage
{
    public MessageType Type { get; set; }
}

// En klass som representerar ett chattedelande. Ärver från WebSocketMessage för att ha en 'Type' egenskap.
public class ChatMessage : WebSocketMessage
{
    // Innehållet i chattedelandet.
    public string Content { get; set; }
    // Avsändaren av chattedelandet.
    public string Sender { get; set; }
}