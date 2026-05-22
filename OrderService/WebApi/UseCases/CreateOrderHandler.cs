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

    public CreateOrderHandler(AppDbContext context, IPaymentServiceClient paymentClient)
    {
        _context = context;
        _paymentClient = paymentClient;
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

        // вызов PaymentService для резервирования
        try
        {
            await _paymentClient.ReservePaymentAsync(
                new ReservePaymentRequest(order.Id, order.Price), cancellationToken);
        }
        catch (Exception ex)
        {
            
        }

        return OrderMapper.ToDto(order);
    }
}