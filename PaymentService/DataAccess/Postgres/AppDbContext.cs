using Microsoft.EntityFrameworkCore;
using PaymentService.DataAccess.Postgres.Models;

namespace PaymentService.DataAccess.Postgres;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Payment> Payments { get; set; }
}