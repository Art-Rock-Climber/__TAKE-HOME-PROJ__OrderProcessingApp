using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using PaymentService.DataAccess.Postgres;
using PaymentService.WebApi.UseCases;

namespace PaymentService.Tests;

[TestFixture]
public class PaymentUseCaseTests
{
    [Test]
    public async Task CreatePayment_SavesPaymentWithPendingStatus()
    {
        await using var context = CreateContext();
        var producer = new TestProducer();
        var handler = new CreatePaymentHandler(
            context,
            producer,
            NullLogger<CreatePaymentHandler>.Instance);

        var result = await handler.Handle(
            new CreatePaymentCommand { OrderId = 77, Price = 250m },
            CancellationToken.None);

        var saved = await context.Payments.SingleAsync();
        Assert.That(result.Id, Is.EqualTo(saved.Id));
        Assert.That(result.OrderId, Is.EqualTo(77));
        Assert.That(result.Price, Is.EqualTo(250m));
        Assert.That(result.Status, Is.False);
    }

    [Test]
    public async Task GetPayment_WhenPaymentExists_ReturnsDto()
    {
        await using var context = CreateContext();
        context.Payments.Add(new() { OrderId = 8, Price = 33m, Status = false });
        await context.SaveChangesAsync();
        var saved = await context.Payments.SingleAsync();
        var handler = new GetPaymentHandler(context);

        var result = await handler.Handle(new GetPaymentQuery { PaymentId = saved.Id }, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(saved.Id));
        Assert.That(result.OrderId, Is.EqualTo(8));
    }

    [Test]
    public async Task GetPayment_WhenPaymentDoesNotExist_ReturnsNull()
    {
        await using var context = CreateContext();
        var handler = new GetPaymentHandler(context);

        var result = await handler.Handle(new GetPaymentQuery { PaymentId = 404 }, CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdatePaymentStatus_WhenPaymentExists_UpdatesStatusAndPublishesEvent()
    {
        await using var context = CreateContext();
        context.Payments.Add(new() { OrderId = 11, Price = 42m, Status = false });
        await context.SaveChangesAsync();
        var saved = await context.Payments.SingleAsync();
        var producer = new TestProducer();
        var handler = new UpdatePaymentStatusHandler(
            context,
            producer,
            NullLogger<CreatePaymentHandler>.Instance);

        var result = await handler.Handle(
            new UpdatePaymentStatusCommand { PaymentId = saved.Id, Status = true },
            CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Status, Is.True);
        Assert.That((await context.Payments.SingleAsync()).Status, Is.True);
        Assert.That(producer.ProducedTopic, Is.EqualTo("payment-events"));
        Assert.That(producer.ProducedMessage?.Key, Is.EqualTo(saved.OrderId.ToString()));
        Assert.That(producer.ProducedMessage?.Value, Does.Contain($"\"PaymentId\":{saved.Id}"));
    }

    [Test]
    public async Task UpdatePaymentStatus_WhenStatusIsFalse_DoesNotPublishEvent()
    {
        await using var context = CreateContext();
        context.Payments.Add(new() { OrderId = 12, Price = 64m, Status = true });
        await context.SaveChangesAsync();
        var saved = await context.Payments.SingleAsync();
        var producer = new TestProducer();
        var handler = new UpdatePaymentStatusHandler(
            context,
            producer,
            NullLogger<CreatePaymentHandler>.Instance);

        var result = await handler.Handle(
            new UpdatePaymentStatusCommand { PaymentId = saved.Id, Status = false },
            CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Status, Is.False);
        Assert.That(producer.ProducedMessage, Is.Null);
    }

    [Test]
    public async Task UpdatePaymentStatus_WhenPaymentDoesNotExist_ReturnsNull()
    {
        await using var context = CreateContext();
        var handler = new UpdatePaymentStatusHandler(
            context,
            new TestProducer(),
            NullLogger<CreatePaymentHandler>.Instance);

        var result = await handler.Handle(
            new UpdatePaymentStatusCommand { PaymentId = 500, Status = true },
            CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private sealed class TestProducer : IProducer<string, string>
    {
        public string Name => nameof(TestProducer);
        public Handle Handle => throw new NotSupportedException();
        public string? ProducedTopic { get; private set; }
        public Message<string, string>? ProducedMessage { get; private set; }

        public int AddBrokers(string brokers) => 0;
        public void AbortTransaction() { }
        public void AbortTransaction(TimeSpan timeout) { }
        public void BeginTransaction() { }
        public void CommitTransaction() { }
        public void CommitTransaction(TimeSpan timeout) { }
        public void Dispose() { }
        public int Flush(TimeSpan timeout) => 0;
        public void Flush(CancellationToken cancellationToken = default) { }
        public Metadata GetMetadata(TimeSpan timeout) => throw new NotSupportedException();
        public Metadata GetMetadata(string topic, TimeSpan timeout) => throw new NotSupportedException();
        public void InitTransactions(TimeSpan timeout) { }
        public void OAuthBearerSetToken(string tokenValue, long lifetimeMs, string principalName, IDictionary<string, string> extensions) { }
        public void OAuthBearerSetTokenFailure(string error) { }
        public int Poll(TimeSpan timeout) => 0;

        public void Produce(
            string topic,
            Message<string, string> message,
            Action<DeliveryReport<string, string>>? deliveryHandler = null)
        {
            ProducedTopic = topic;
            ProducedMessage = message;
        }

        public void Produce(
            TopicPartition topicPartition,
            Message<string, string> message,
            Action<DeliveryReport<string, string>>? deliveryHandler = null)
        {
            ProducedTopic = topicPartition.Topic;
            ProducedMessage = message;
        }

        public Task<DeliveryResult<string, string>> ProduceAsync(
            string topic,
            Message<string, string> message,
            CancellationToken cancellationToken = default)
        {
            ProducedTopic = topic;
            ProducedMessage = message;
            return Task.FromResult(new DeliveryResult<string, string>());
        }

        public Task<DeliveryResult<string, string>> ProduceAsync(
            TopicPartition topicPartition,
            Message<string, string> message,
            CancellationToken cancellationToken = default)
        {
            ProducedTopic = topicPartition.Topic;
            ProducedMessage = message;
            return Task.FromResult(new DeliveryResult<string, string>());
        }

        public void SendOffsetsToTransaction(
            IEnumerable<TopicPartitionOffset> offsets,
            IConsumerGroupMetadata groupMetadata,
            TimeSpan timeout)
        {
        }

        public void SetSaslCredentials(string username, string password) { }
    }
}
