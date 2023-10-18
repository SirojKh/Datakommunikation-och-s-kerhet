using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using Microsoft.Extensions.Logging;

public class Program
{
    private static SignalRService _signalRService;
    private static SortedDictionary<string, string> _temperatures = new SortedDictionary<string, string>();

    public static async Task Main(string[] args)
    {
        _signalRService = new SignalRService();
        _signalRService.OnTemperatureReceived += UpdateTemperature;

        await _signalRService.StartConnectionAsync();

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();

        await _signalRService.StopConnectionAsync();
    }

    private static void UpdateTemperature(string device, string temperature)
    {
        _temperatures[device] = temperature;
        DisplayTemperatures();
    }

    private static void DisplayTemperatures()
    {
        Console.Clear();
        Console.WriteLine("Temperature Monitor");
        foreach (var entry in _temperatures)
        {
            Console.WriteLine($"{entry.Key}: Temperature: {entry.Value} °C");
        }
    }
}

// ... [SignalRService class here, unchanged] ...


public class SignalRService
{
    private readonly HubConnection _hubConnection;
    private readonly object _lock = new object();

    // Händelse som utlöses när en temperatur mottas
    public event Action<string, string> OnTemperatureReceived
    {
        add
        {
            lock (_lock)
            {
                _onTemperatureReceived += value;
            }
        }
        remove
        {
            lock (_lock)
            {
                _onTemperatureReceived -= value;
            }
        }
    }
    private event Action<string, string> _onTemperatureReceived;

    public SignalRService()
    {
        // Konfigurera SignalR-hubanslutning
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:5001/temperatureHub")
            .WithAutomaticReconnect()
            .ConfigureLogging(logging => 
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();

        // Hantera mottagna temperaturmeddelanden
        _hubConnection.On<string, string>("ReceiveTemperature", (device, temperature) =>
        {
            _onTemperatureReceived?.Invoke(device, temperature);
            Console.WriteLine($"Temperature Received: Device: {device}, Temperature: {temperature}°C");
        });

        // Hantera anslutningshändelser
        _hubConnection.Closed += (error) =>
        {
            Console.WriteLine($"Connection Closed. Error: {error?.Message}");
            return Task.CompletedTask;
        };

        _hubConnection.Reconnecting += (error) =>
        {
            Console.WriteLine($"Attempting to reconnect. Error: {error?.Message}");
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += (connectionId) =>
        {
            Console.WriteLine($"Reconnected with Connection ID: {connectionId}");
            return Task.CompletedTask;
        };
    }

    // Starta SignalR-anslutning
    public async Task StartConnectionAsync()
    {
        try
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                await _hubConnection.StartAsync();
                Console.WriteLine("Connection Started");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Kunde inte starta anslutningen: " + e.Message);
        }
    }

    // Stoppa SignalR-anslutning
    public async Task StopConnectionAsync()
    {
        try
        {
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.StopAsync();
                Console.WriteLine("Connection Stopped");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Kunde inte stoppa anslutningen: " + e.Message);
        }
    }
}