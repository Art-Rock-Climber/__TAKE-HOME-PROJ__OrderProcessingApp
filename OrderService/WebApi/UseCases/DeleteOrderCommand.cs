using MediatR;

namespace OrderService.WebApi.UseCases;

public class DeleteOrderCommand : IRequest<bool>
{
    public long Id { get; set; }
}