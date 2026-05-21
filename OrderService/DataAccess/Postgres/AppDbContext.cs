using Microsoft.EntityFrameworkCore;
using OrderService.DataAccess.Postgres.Models;

namespace OrderService.DataAccess.Postgres;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Order> Orders { get; set; }
}
