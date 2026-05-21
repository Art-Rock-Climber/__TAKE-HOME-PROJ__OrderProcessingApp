namespace PaymentService.DataAccess.Postgres.Models;

public class Payment
{
    public int Id { get; set; }
    public long OrderId { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed
    public DateTime DateCreate { get; set; } = DateTime.UtcNow;
}
