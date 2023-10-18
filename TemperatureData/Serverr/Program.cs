using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security.Cryptography;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using Microsoft.Extensions.Logging;

public class Serverr
{
    public static void Main(string[] args)
    {
        var host = new HostBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>()
                    .UseUrls("http://localhost:5000", "https://localhost:5001")
                    .ConfigureLogging(logging => 
                    {
                        logging.ClearProviders();
                        logging.AddConsole();
                    });
            })
            .Build();

        host.Run();
    }
}

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSignalR();

        // CORS (Cross-Origin Resource Sharing) för att tillåta anslutningar från specifika ursprung
        services.AddCors(options =>
        {
            options.AddPolicy("AllowWebAppOrigins", builder =>
            {
                builder.WithOrigins("https://localhost:5001")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseCors("AllowWebAppOrigins");

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<TemperatureHub>("/temperatureHub");
        });
    }
}

public class TemperatureHub : Hub
{
    // ... [Unchanged code from your TemperatureHub class] ...
    
    // Logger för att logga händelser och fel
    private readonly ILogger<TemperatureHub> _logger;
    
    // Krypteringsnyckel och initialvektor för AES-dekryptering
    private static readonly byte[] Key = Convert.FromBase64String("EzErv+w4W9IOb9FCcK1wQ68Xml7PHD6KHIuJ+6bS/o4=");
    private static readonly byte[] IV = Convert.FromBase64String("1IQ+GChzn/9Rr8MWfAH1Sw==");
    
    public TemperatureHub(ILogger<TemperatureHub> logger)
    {
        _logger = logger;
    }

    // Metod för att ta emot och behandla temperaturdata
    public async Task SendTemperature(string serializedTemperatureData)
    {
        try
        {
            // Deserialisera temperaturdata
            var temperatureData = JsonSerializer.Deserialize<TemperatureData>(serializedTemperatureData);
            
            // Kontrollera om datan är giltig
            if (temperatureData == null)
            {
                _logger.LogError("Inkommande data kunde inte deserialiseras korrekt.");
                return;
            }

            if (string.IsNullOrWhiteSpace(temperatureData.DeviceName))
            {
                _logger.LogError("Enhetens namn är ogiltigt eller saknas.");
                return;
            }

            if (string.IsNullOrWhiteSpace(temperatureData.EncryptedTemperature))
            {
                _logger.LogError("Krypterad temperatur är ogiltig eller saknas.");
                return;
            }

            // Dekryptera temperaturdata
            string decryptedTemperature = DecryptString(temperatureData.EncryptedTemperature);
            _logger.LogInformation($"Mottagen temperatur från {temperatureData.DeviceName}: {decryptedTemperature}°C");

            // Skicka temperaturdata till alla anslutna klienter
            await Clients.All.SendAsync("ReceiveTemperature", temperatureData.DeviceName, decryptedTemperature);
            _logger.LogInformation($"Skickade temperatur till alla anslutna klienter: {decryptedTemperature}°C");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ett fel inträffade när temperaturen skulle skickas.");
            throw;
        }
    }

    // Funktion för att dekryptera en sträng med AES-dekryptering
    static string DecryptString(string cipherText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Key;
            aes.IV = IV;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
            {
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }
    
}

public class TemperatureData
{
    public string? DeviceName { get; set; }
    public string? EncryptedTemperature { get; set; }
}