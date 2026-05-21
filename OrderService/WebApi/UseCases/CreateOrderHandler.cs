using MediatR;
using OrderService.DataAccess.Postgres;
using OrderService.DataAccess.Postgres.Models;
using OrderService.WebApi.Controllers;

namespace OrderService.WebApi.UseCases;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly AppDbContext _context;

    public CreateOrderHandler(AppDbContext context)
    {
        _context = context;
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

        return OrderMapper.ToDto(order);
    }
}