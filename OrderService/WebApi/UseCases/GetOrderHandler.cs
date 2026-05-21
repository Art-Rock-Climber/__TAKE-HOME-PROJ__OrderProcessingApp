using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.DataAccess.Postgres;
using OrderService.WebApi.Controllers;

namespace OrderService.WebApi.UseCases;

public class GetOrderHandler : IRequestHandler<GetOrderQuery, OrderDto?>
{
    private readonly AppDbContext _context;

    public GetOrderHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto?> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FindAsync(request.Id, cancellationToken);
        if (order == null) return null;

        return OrderMapper.ToDto(order);
    }
}