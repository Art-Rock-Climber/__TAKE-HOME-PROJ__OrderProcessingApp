using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.DataAccess.Postgres;
using OrderService.WebApi.Controllers;

namespace OrderService.WebApi.UseCases;

public class GetOrderHandler : IRequestHandler<GetOrderQuery, OrderDto?>
{
    private readonly AppDbContext _context;
    private readonly ILogger<GetOrderHandler> _logger; 

    public GetOrderHandler(AppDbContext context, ILogger<GetOrderHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<OrderDto?> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FindAsync(request.Id, cancellationToken);
        if (order == null) return null;

        _logger.LogInformation("✅ Order found: Id={Id}", order.Id);

        return OrderMapper.ToDto(order);
    }
}