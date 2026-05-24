using MediatR;
using OrderService.DataAccess.Postgres;

namespace OrderService.WebApi.UseCases;

public class DeleteOrderHandler : IRequestHandler<DeleteOrderCommand, bool>
{
    private readonly AppDbContext _context;
    private readonly ILogger<DeleteOrderHandler> _logger; 


    public DeleteOrderHandler(AppDbContext context, ILogger<DeleteOrderHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FindAsync(request.Id, cancellationToken);
        if (order == null) return false;

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("✅ Order deleted: Id={Id}", order.Id);

        return true;
    }
}