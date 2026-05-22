using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderService.DataAccess.Postgres;
using OrderService.DataAccess.Postgres.Models;
using OrderService.WebApi.UseCases;

namespace OrderService.WebApi.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("create")]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] OrderDto request)
    {
        var command = new CreateOrderCommand
        {
            ProductId = request.ProductId,
            Amount = request.Amount,
            EmailClient = request.EmailClient,
            Price = request.Price,
            PhoneNumber = request.PhoneNumber
        };
        
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(long id)
    {
        var result = await _mediator.Send(new GetOrderQuery { Id = id });
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(long id)
    {
        var result = await _mediator.Send(new DeleteOrderCommand { Id = id });
        if (!result) return NotFound();
        return NoContent();
    }
}

