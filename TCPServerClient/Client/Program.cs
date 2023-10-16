using System;
using System.IO;
using System.Net.Sockets;
using Newtonsoft.Json;

public class Client
{
    public static void Main()
    {
        // Ansluter till servern på port 8080
        TcpClient client = new TcpClient("127.0.0.1", 8080);
        StreamWriter writer = new StreamWriter(client.GetStream());
        StreamReader reader = new StreamReader(client.GetStream());

        // Skapar en begäran och skickar den till servern
        Console.Write("Enter a message: ");
        string content = Console.ReadLine();
        Request request = new Request
        {
            Content = content
        };

        string message = JsonConvert.SerializeObject(request);
        writer.WriteLine(message);
        writer.Flush();

        // Läser svaret från servern
        string responseMessage = reader.ReadLine();
        Response response = JsonConvert.DeserializeObject<Response>(responseMessage);
        Console.WriteLine($"Server responded: {response.Content}");

        client.Close();
    }
}