using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.DataAccess.Postgres.Models;

namespace OrderService.DataAccess.Postgres;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        
        builder.HasKey(o => o.Id);
        
        builder.Property(o => o.ProductId)
            .IsRequired();
            
        builder.Property(o => o.Amount)
            .IsRequired();
            
        builder.Property(o => o.EmailClient)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(o => o.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
            
        builder.Property(o => o.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);
    }
}