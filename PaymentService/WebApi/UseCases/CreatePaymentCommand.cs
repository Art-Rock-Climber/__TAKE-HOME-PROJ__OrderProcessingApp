using MediatR;
using PaymentService.WebApi.Controllers;

namespace PaymentService.WebApi.UseCases;

public class CreatePaymentCommand : IRequest<PaymentDto>
{
    public long OrderId { get; set; }
    public decimal Price { get; set; }
    public DateTime DateCreate { get; set; } = DateTime.UtcNow;
}