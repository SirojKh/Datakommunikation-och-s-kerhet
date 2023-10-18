using Server.Models;


var builder = WebApplication.CreateBuilder(args);

//Services
builder.Services.AddSingleton<WebSocketManager>();
builder.Services.AddTransient<MessageProcessor>();

//Models
builder.Services.AddSingleton<WebSocketMessage>();


var app = builder.Build();


// Middleware to handle WebSocket requests
app.UseWebSockets();
app.UseMiddleware<WebSocketMiddleware>();

app.MapGet("/", () => "WebSocket Server is Running!");

app.Run();