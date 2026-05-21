using MediatR;
using OrderService.WebApi.Controllers;

namespace OrderService.WebApi.UseCases;

public class GetOrderQuery : IRequest<OrderDto?>
{
    public int Id { get; set; }
}