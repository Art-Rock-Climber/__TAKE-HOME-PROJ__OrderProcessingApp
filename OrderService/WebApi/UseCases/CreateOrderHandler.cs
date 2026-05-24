using MediatR;
using OrderService.DataAccess.Postgres;
using OrderService.DataAccess.Postgres.Models;
using OrderService.WebApi.Controllers;
using OrderService.WebApi.Clients;

namespace OrderService.WebApi.UseCases;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly AppDbContext _context;
    private readonly IPaymentServiceClient _paymentClient;
    private readonly ILogger<CreateOrderHandler> _logger; 

    public CreateOrderHandler(AppDbContext context, IPaymentServiceClient paymentClient, ILogger<CreateOrderHandler> logger)
    {
        _context = context;
        _paymentClient = paymentClient;
        _logger = logger;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            ProductId = request.ProductId,
            Amount = request.Amount,
            EmailClient = request.EmailClient,
            Price = request.Price,
            PhoneNumber = request.PhoneNumber,
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("✅ Order saved to DB: Id={Id}", order.Id);

        // вызов PaymentService для резервирования
        try
        {
            await _paymentClient.ReservePaymentAsync(
                new ReservePaymentRequest(order.Id, order.Price), cancellationToken);
            _logger.LogInformation("✅ Payment reserved");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ PaymentService reserving ERROR");
        }

        return OrderMapper.ToDto(order);
    }
}