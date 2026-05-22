using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentService.WebApi.Controllers;
using PaymentService.WebApi.UseCases;

namespace PaymentService.WebApi.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("create")]
    public async Task<ActionResult<PaymentDto>> CreatePayment([FromBody] CreatePaymentCommand command)
    {
        var result = await _mediator.Send(command);
        return Created($"/api/payments/get/{result.OrderId}", result);
    }

    [HttpPut("updateStatus/{paymentId}/{status}")]
    public async Task<ActionResult<PaymentDto>> UpdatePaymentStatus(
        long paymentId, bool status, [FromBody] UpdatePaymentStatusCommand command)
    {
        // Синхронизация параметров
        command.PaymentId = paymentId;
        command.Status = status;
        
        var result = await _mediator.Send(command);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("get/{paymentId}")]
    public async Task<ActionResult<PaymentDto>> GetPayment(long paymentId)
    {
        var result = await _mediator.Send(new GetPaymentQuery { PaymentId = paymentId });
        return result is null ? NotFound() : Ok(result);
    }
}