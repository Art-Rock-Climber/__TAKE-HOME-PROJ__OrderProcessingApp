using Microsoft.AspNetCore.SignalR.Client;

Console.WriteLine("🔄 Подключение к NotificationService...");

var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5005/notifications")
    .WithAutomaticReconnect()
    .Build();

connection.On<object>("ReceiveNotification", (data) =>
{
    Console.WriteLine($"🔔 [{DateTime.Now:HH:mm:ss}] {data}");
});

connection.Closed += async (error) =>
{
    Console.WriteLine($"❌ Соединение закрыто: {error?.Message}");
    await Task.Delay(2000);
};

try
{
    await connection.StartAsync();
    Console.WriteLine("✅ Подключено! Жду уведомления...");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Ошибка: {ex.Message}");
    return;
}

// Держим соединение
while (true) { await Task.Delay(1000); }