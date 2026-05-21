using MediatR;

namespace OrderService.WebApi.UseCases;

public class DeleteOrderCommand : IRequest<bool>
{
    public int Id { get; set; }
}