namespace PaymentService.DataAccess.Postgres.Models;

public class Payment
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public decimal Price { get; set; }
    public bool Status { get; set; } = false;
    public DateTime DateCreate { get; set; } = DateTime.UtcNow;
}
