using Microsoft.EntityFrameworkCore;
using MiniErp.Models;

namespace MiniErp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(customer => customer.TaxId).IsUnique();
            entity.Property(customer => customer.Name).IsUnicode();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(product => product.Sku).IsUnique();
            entity.Property(product => product.UnitPrice).HasPrecision(18, 2);
            entity.Property(product => product.Stock).HasPrecision(18, 3);
        });

        modelBuilder.Entity<SalesOrder>(entity =>
        {
            entity.HasIndex(order => order.Number).IsUnique();
            entity.HasOne(order => order.Customer)
                .WithMany(customer => customer.SalesOrders)
                .HasForeignKey(order => order.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SalesOrderLine>(entity =>
        {
            entity.Property(line => line.Quantity).HasPrecision(18, 3);
            entity.Property(line => line.UnitPrice).HasPrecision(18, 2);
            entity.Property(line => line.DiscountPercentage).HasPrecision(5, 2);
            entity.HasOne(line => line.SalesOrder)
                .WithMany(order => order.Lines)
                .HasForeignKey(line => line.SalesOrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(line => line.Product)
                .WithMany(product => product.SalesOrderLines)
                .HasForeignKey(line => line.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
