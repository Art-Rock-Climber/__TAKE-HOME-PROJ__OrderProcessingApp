using Refit;

namespace OrderService.WebApi.Clients;

public interface IPaymentServiceClient
{
    [Post("/api/payments/create")]
    Task<PaymentReservationResponse> ReservePaymentAsync([Body] ReservePaymentRequest request, CancellationToken ct = default);
}

public record ReservePaymentRequest(long OrderId, decimal Price);
public record PaymentReservationResponse(decimal Price, bool Status, DateTime DateCreate);