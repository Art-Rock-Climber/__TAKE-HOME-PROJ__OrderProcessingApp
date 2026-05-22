using MediatR;
using PaymentService.DataAccess.Postgres;
using PaymentService.DataAccess.Postgres.Models;
using PaymentService.WebApi.Controllers;

namespace PaymentService.WebApi.UseCases;

public class CreatePaymentHandler : IRequestHandler<CreatePaymentCommand, PaymentDto>
{
    private readonly AppDbContext _context;

    public CreatePaymentHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentDto> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = new Payment
        {
            OrderId = request.OrderId,
            Price = request.Price,
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        return PaymentMapper.ToDto(payment);
    }
}