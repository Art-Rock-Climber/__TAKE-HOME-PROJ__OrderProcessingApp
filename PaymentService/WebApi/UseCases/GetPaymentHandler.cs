using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentService.DataAccess.Postgres;
using PaymentService.WebApi.Controllers;

namespace PaymentService.WebApi.UseCases;

public class GetPaymentHandler : IRequestHandler<GetPaymentQuery, PaymentDto?>
{
    private readonly AppDbContext _context;

    public GetPaymentHandler(AppDbContext context) => _context = context;

    public async Task<PaymentDto?> Handle(GetPaymentQuery request, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments.FindAsync(request.PaymentId, cancellationToken);
        if (payment == null) return null;

        return PaymentMapper.ToDto(payment);
    }
}