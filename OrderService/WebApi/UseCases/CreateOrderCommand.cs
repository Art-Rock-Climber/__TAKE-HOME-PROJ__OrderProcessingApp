using MediatR;
using OrderService.WebApi.Controllers;

namespace OrderService.WebApi.UseCases;

public class CreateOrderCommand : IRequest<OrderDto>
{
    public long ProductId { get; set; }
    public int Amount { get; set; }
    public string EmailClient { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
}