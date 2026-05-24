using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentService.DataAccess.Postgres;
using PaymentService.WebApi.Controllers;

using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

using Microsoft.Extensions.Logging;

namespace PaymentService.WebApi.UseCases;

public class UpdatePaymentStatusHandler : IRequestHandler<UpdatePaymentStatusCommand, PaymentDto?>
{
    private readonly AppDbContext _context;
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<CreatePaymentHandler> _logger; 

    public UpdatePaymentStatusHandler(
        AppDbContext context, 
        IProducer<string, string> producer,
        ILogger<CreatePaymentHandler> logger)
    {
        _context = context;
        _producer = producer;
        _logger = logger;
    }

    public async Task<PaymentDto?> Handle(UpdatePaymentStatusCommand request, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments.FindAsync(new object[] { request.PaymentId }, cancellationToken);
        if (payment == null) return null;

        payment.Status = request.Status;
        
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("✅ Payment updated: Id={Id}", payment.Id);

        if (payment.Status) // при успешной оплате
        {
            // Публикация события
            var evt = new { PaymentId = payment.Id, OrderId = payment.OrderId, Price = payment.Price, Status = payment.Status };
            var message = new Message<string, string>
            {
                Key = payment.OrderId.ToString(),
                Value = System.Text.Json.JsonSerializer.Serialize(evt)
            };

            try
            {
                await _producer.ProduceAsync("payment-events", message, cancellationToken);
                _logger.LogInformation("📤 Kafka: message sent to payment-events");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Kafka ERROR");
            }
        }

        return PaymentMapper.ToDto(payment);
    }
}