using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

public class ClientA
{
    public static void Main()
    {
        // Skapa en UDP-klient för att sända data
        UdpClient udpClient = new UdpClient();

        // Mål IP och port för att sända meddelandet
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);

        // Skapa och serialisera objektet
        Console.Write("Enter a message: ");
        string content = Console.ReadLine();
        Message message = new Message { Content = content };
        string json = JsonConvert.SerializeObject(message);

        // Skicka meddelandet
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        udpClient.Send(bytes, bytes.Length, endPoint);

        udpClient.Close();
    }
}

public class Message
{
    public string Content { get; set; }
}