using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;
using System.Text.Json;

namespace NotificationService.Consumers;

public class KafkaConsumerService : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly string[] _topics;
    private readonly ILogger<KafkaConsumerService> _logger; 

    public KafkaConsumerService(IConfiguration config, IHubContext<NotificationHub> hubContext, ILogger<KafkaConsumerService> logger)
    {
        _hubContext = hubContext;
        _topics = config["Kafka:Topics"]?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) 
                ?? Array.Empty<string>();
        _logger = logger;

        var kafkaConfig = new ConsumerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            GroupId = config["Kafka:GroupId"] ?? "notification-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        _consumer = new ConsumerBuilder<string, string>(kafkaConfig).Build();
        _consumer.Subscribe(_topics);

        _logger.LogInformation("✅ KafkaConsumer: subscribed successfully");
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔄 KafkaConsumer: ExecuteAsync started");

        return Task.Run(() =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(cancellationToken);
                    if (result?.Message?.Value != null)
                    {
                        _logger.LogInformation("📥 Kafka: received from {Topic}: {Payload}", 
                                                    result.Topic, result.Message.Value);

                        var notification = new { Topic = result.Topic, Payload = result.Message.Value };

                        // Рассылка через WebSocket
                        _hubContext.Clients.All.SendAsync("ReceiveNotification", notification, cancellationToken);

                        _logger.LogInformation("📤 SignalR: sent to clients");
                    }
                }
                catch (ConsumeException e)
                {
                    _logger.LogError(e, "❌ Kafka consume error: {Error}", e.Error.Reason);
                }
            }
        }, cancellationToken);
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        base.Dispose();
    }
}