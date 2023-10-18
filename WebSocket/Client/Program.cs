using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("WebSocket Client");

            using var client = new ClientWebSocket();

            try
            {
                // Anslut till WebSocket-servern
                var serverUri = new Uri("ws://localhost:5230"); // Ändra till din WebSocket-server-URI
                await client.ConnectAsync(serverUri, CancellationToken.None);

                // Starta en separat task för att lyssna på meddelanden från servern
                _ = Task.Run(async () => {
                    var buffer = new byte[1024];
                    while (client.State == WebSocketState.Open)
                    {
                        var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine($"Received from server: {receivedMessage}");
                    }
                });

                while (client.State == WebSocketState.Open)
                {
                    Console.WriteLine("Enter message (or type 'exit' to quit):");
                    var message = Console.ReadLine();

                    if (message?.ToLower() == "exit")
                    {
                        await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client requested exit", CancellationToken.None);
                        break;
                    }

                    // Skapa ett meddelandeobjekt och serialisera det till JSON
                    var payload = new
                    {
                        content = message
                    };
                    var serializedMessage = JsonSerializer.Serialize(payload);

                    // Skicka meddelandet till servern
                    var bytes = Encoding.UTF8.GetBytes(serializedMessage);
                    await client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebSocket Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                if (client.State != WebSocketState.Closed && client.State != WebSocketState.Aborted)
                    await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing client", CancellationToken.None);
            }
        }
    }
}
