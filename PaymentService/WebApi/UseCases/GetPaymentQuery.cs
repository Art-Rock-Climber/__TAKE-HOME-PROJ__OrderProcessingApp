using MediatR;
using PaymentService.WebApi.Controllers;

namespace PaymentService.WebApi.UseCases;

public class GetPaymentQuery : IRequest<PaymentDto?>
{
    public long PaymentId { get; set; }
}