using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

public class ClientB
{
    public static void Main()
    {
        // Skapa en UDP-klient för att lyssna på en specifik port
        UdpClient udpListner = new UdpClient(8080);

        Console.WriteLine("Waiting for a message...");

        // Ta emot meddelandet
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
        byte[] bytes = udpListner.Receive(ref endPoint);
        string json = Encoding.UTF8.GetString(bytes);

        // Deserialisera objektet
        Message message = JsonConvert.DeserializeObject<Message>(json);

        Console.WriteLine($"Received: {message.Content}");

        udpListner.Close();
    }
}