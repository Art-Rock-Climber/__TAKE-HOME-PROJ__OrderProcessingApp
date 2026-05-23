using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentService.WebApi.Controllers;
using PaymentService.WebApi.UseCases;
using Microsoft.Extensions.Logging;

namespace PaymentService.WebApi.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentsController> _logger; 

    public PaymentsController(IMediator mediator, ILogger<PaymentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("create")]
    public async Task<ActionResult<PaymentDto>> CreatePayment([FromBody] CreatePaymentCommand command)
    {
        System.Diagnostics.Debug.WriteLine($"🎯 DEBUG: CreatePayment called with OrderId={command.OrderId}");
        _logger.LogInformation("🎯 Controller received: OrderId={OrderId}, Price={Price}", command.OrderId, command.Price);

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