using MediatR;
using PaymentService.WebApi.Controllers;

namespace PaymentService.WebApi.UseCases;

public class UpdatePaymentStatusCommand : IRequest<PaymentDto?>
{
    public long PaymentId { get; set; }
    public bool Status { get; set; }
}