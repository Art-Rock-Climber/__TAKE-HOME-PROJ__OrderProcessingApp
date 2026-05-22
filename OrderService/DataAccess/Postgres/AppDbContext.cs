using Microsoft.EntityFrameworkCore;
using OrderService.DataAccess.Postgres.Models;

namespace OrderService.DataAccess.Postgres;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Order> Orders { get; set; }
}
