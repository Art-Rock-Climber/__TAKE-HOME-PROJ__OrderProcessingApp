using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentService.DataAccess.Postgres;
using PaymentService.WebApi.Controllers;

namespace PaymentService.WebApi.UseCases;

public class UpdatePaymentStatusHandler : IRequestHandler<UpdatePaymentStatusCommand, PaymentDto?>
{
    private readonly AppDbContext _context;

    public UpdatePaymentStatusHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentDto?> Handle(UpdatePaymentStatusCommand request, CancellationToken ct)
    {
        var payment = await _context.Payments.FindAsync(new object[] { request.PaymentId }, ct);
        if (payment == null) return null;

        payment.Status = request.Status;
        
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync(ct);
        
        return PaymentMapper.ToDto(payment);
    }
}