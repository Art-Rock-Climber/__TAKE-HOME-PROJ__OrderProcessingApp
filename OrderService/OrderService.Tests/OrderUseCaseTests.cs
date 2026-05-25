using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using OrderService.DataAccess.Postgres;
using OrderService.Tests.Fakes;
using OrderService.WebApi.Clients;
using OrderService.WebApi.UseCases;

namespace OrderService.Tests;

[TestFixture]
public class OrderUseCaseTests
{
    [Test]
    public async Task CreateOrder_SavesOrderAndReservesPayment()
    {
        await using var context = CreateContext();
        var paymentClient = new TestPaymentServiceClient();
        var handler = new CreateOrderHandler(
            context,
            paymentClient,
            NullLogger<CreateOrderHandler>.Instance);
        var command = new CreateOrderCommand
        {
            ProductId = 42,
            Amount = 3,
            EmailClient = "client@example.com",
            Price = 125.50m,
            PhoneNumber = "+79991234567"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        var saved = await context.Orders.SingleAsync();
        Assert.That(result.Id, Is.EqualTo(saved.Id));
        Assert.That(result.ProductId, Is.EqualTo(command.ProductId));
        Assert.That(saved.Amount, Is.EqualTo(command.Amount));
        Assert.That(paymentClient.LastRequest, Is.EqualTo(new ReservePaymentRequest(saved.Id, command.Price)));
    }

    [Test]
    public async Task CreateOrder_WhenPaymentClientFails_StillReturnsSavedOrder()
    {
        await using var context = CreateContext();
        var handler = new CreateOrderHandler(
            context,
            new TestPaymentServiceClient(throwOnReserve: true),
            NullLogger<CreateOrderHandler>.Instance);

        var result = await handler.Handle(
            new CreateOrderCommand { ProductId = 1, Amount = 1, Price = 10m },
            CancellationToken.None);

        Assert.That(result.Id, Is.GreaterThan(0));
        Assert.That(await context.Orders.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetOrder_WhenOrderExists_ReturnsDto()
    {
        await using var context = CreateContext();
        context.Orders.Add(new() { ProductId = 8, Amount = 4, EmailClient = "a@b.com", Price = 77m, PhoneNumber = "+12345" });
        await context.SaveChangesAsync();
        var saved = await context.Orders.SingleAsync();
        var handler = new GetOrderHandler(context, NullLogger<GetOrderHandler>.Instance);

        var result = await handler.Handle(new GetOrderQuery { Id = saved.Id }, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(saved.Id));
        Assert.That(result.Price, Is.EqualTo(77m));
    }

    [Test]
    public async Task GetOrder_WhenOrderDoesNotExist_ReturnsNull()
    {
        await using var context = CreateContext();
        var handler = new GetOrderHandler(context, NullLogger<GetOrderHandler>.Instance);

        var result = await handler.Handle(new GetOrderQuery { Id = 123 }, CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task DeleteOrder_WhenOrderExists_RemovesOrder()
    {
        await using var context = CreateContext();
        context.Orders.Add(new() { ProductId = 2, Amount = 1, Price = 15m });
        await context.SaveChangesAsync();
        var saved = await context.Orders.SingleAsync();
        var handler = new DeleteOrderHandler(context, NullLogger<DeleteOrderHandler>.Instance);

        var deleted = await handler.Handle(new DeleteOrderCommand { Id = saved.Id }, CancellationToken.None);

        Assert.That(deleted, Is.True);
        Assert.That(await context.Orders.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task DeleteOrder_WhenOrderDoesNotExist_ReturnsFalse()
    {
        await using var context = CreateContext();
        var handler = new DeleteOrderHandler(context, NullLogger<DeleteOrderHandler>.Instance);

        var deleted = await handler.Handle(new DeleteOrderCommand { Id = 321 }, CancellationToken.None);

        Assert.That(deleted, Is.False);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private sealed class TestPaymentServiceClient : IPaymentServiceClient
    {
        private readonly bool _throwOnReserve;

        public TestPaymentServiceClient(bool throwOnReserve = false)
        {
            _throwOnReserve = throwOnReserve;
        }

        public ReservePaymentRequest? LastRequest { get; private set; }

        public Task<PaymentReservationResponse> ReservePaymentAsync(
            ReservePaymentRequest request,
            CancellationToken ct = default)
        {
            if (_throwOnReserve)
            {
                throw new InvalidOperationException("Payment service is unavailable.");
            }

            LastRequest = request;
            return Task.FromResult(new PaymentReservationResponse(request.Price, false, DateTime.UtcNow));
        }
    }
}
