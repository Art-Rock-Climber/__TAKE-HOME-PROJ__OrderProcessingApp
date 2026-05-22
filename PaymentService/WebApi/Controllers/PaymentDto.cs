namespace PaymentService.WebApi.Controllers;

public class PaymentDto
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public decimal Price { get; set; }
    public bool Status { get; set; } = false;
    public DateTime DateCreate { get; set; } = DateTime.UtcNow;
}
