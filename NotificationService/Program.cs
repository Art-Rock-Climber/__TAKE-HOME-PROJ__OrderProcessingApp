using NotificationService.Consumers;
using NotificationService.Hubs;
using Microsoft.AspNetCore.SignalR;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddHostedService<KafkaConsumerService>();

builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();

var app = builder.Build();



// health-check эндпоинт
app.MapGet("/", () => "NotificationService is running.");

// WebSocket эндпоинт
app.MapHub<NotificationHub>("/notifications");

app.Run();