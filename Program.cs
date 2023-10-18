using System.Security.Cryptography;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;

class Program
{
    // Krypteringsnyckel och initialvektor för AES-kryptering
    private static readonly byte[] Key = Convert.FromBase64String("EzErv+w4W9IOb9FCcK1wQ68Xml7PHD6KHIuJ+6bS/o4=");
    private static readonly byte[] IV = Convert.FromBase64String("1IQ+GChzn/9Rr8MWfAH1Sw==");
    
    private static readonly List<string> Devices = new List<string> { "Device1", "Device2", "Device3", "Device4", "Device5" };

    static async Task Main(string[] args)
    {
        // Skapa en anslutning till SignalR-hubben
        var hubUrl = "https://localhost:5001/temperatureHub";
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.HttpMessageHandlerFactory = (message) =>
                {
                    if (message is HttpClientHandler clientHandler)
                        clientHandler.ServerCertificateCustomValidationCallback = 
                            (sender, certificate, chain, sslPolicyErrors) => true;
                    return message;
                };
            })
            .Build();

        await hubConnection.StartAsync();

        string[] devices = new string[] { "Device1", "Device2", "Device3", "Device4", "Device5" };
        
        // Skicka temperaturdata för varje enhet med ett intervall på 1 sekund mellan varje enhet
        while (true)
        {
            foreach (var device in devices)
            {
                var random = new Random();
                var temperature = random.Next(-30, 50);

                // Kryptera temperaturdata
                var temperatureData = new TemperatureData
                {
                    DeviceName = device,
                    EncryptedTemperature = EncryptString(temperature.ToString())
                };

                var serializedTemperatureData = JsonSerializer.Serialize(temperatureData);

                await hubConnection.SendAsync("SendTemperature", serializedTemperatureData);
                Console.WriteLine($"Skickade temperaturdata: {serializedTemperatureData}");
                await Task.Delay(1000);
            }

            // Vänta 5 sekunder innan nästa uppdatering
            await Task.Delay(5000);
        }
    }

    // Funktion för att kryptera en sträng med AES-kryptering
    static string EncryptString(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Key;
            aes.IV = IV;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
    }
    
    // Klass för att representera temperaturdata
    public class TemperatureData
    {
        public string? DeviceName { get; set; }
        public string? EncryptedTemperature { get; set; }
    }
}