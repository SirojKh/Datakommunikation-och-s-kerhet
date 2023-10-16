using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;

public class Server
{
    public static void Main()
    {
        // Startar servern på port 8080
        TcpListener server = new TcpListener(IPAddress.Any, 8080);
        server.Start();
        Console.WriteLine("Server started on port 8080...");

        while (true)
        {
            // Väntar på inkommande klientanslutningar
            TcpClient client = server.AcceptTcpClient();
            StreamReader reader = new StreamReader(client.GetStream());
            StreamWriter writer = new StreamWriter(client.GetStream());

            // Läser inkommande meddelanden
            string message = reader.ReadLine();
            Request request = JsonConvert.DeserializeObject<Request>(message);

            // Loggar och behandlar begäran
            Console.WriteLine($"Received: {request.Content}");
            Response response = new Response
            {
                Content = "Received: " + request.Content
            };

            // Svarar klienten
            string responseMessage = JsonConvert.SerializeObject(response);
            writer.WriteLine(responseMessage);
            writer.Flush();
            
            client.Close();
        }
    }
}

public class Request
{
    public string Content { get; set; }
}

public class Response
{
    public string Content { get; set; }
}