using MediatR;
using OrderService.WebApi.Controllers;

namespace OrderService.WebApi.UseCases;

public class GetOrderQuery : IRequest<OrderDto?>
{
    public long Id { get; set; }
}