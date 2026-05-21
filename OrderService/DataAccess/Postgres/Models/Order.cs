namespace OrderService.DataAccess.Postgres.Models;

public class Order
{
    public int Id { get; set; }
    public long ProductId { get; set; }
    public int Amount { get; set; }
    public string EmailClient { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
}
