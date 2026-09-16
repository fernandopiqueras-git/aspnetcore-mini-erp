using Microsoft.EntityFrameworkCore;
using MiniErp.Models;

namespace MiniErp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<WarehouseStock> WarehouseStocks => Set<WarehouseStock>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

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
            entity.HasOne(order => order.Customer).WithMany(customer => customer.SalesOrders).HasForeignKey(order => order.CustomerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(order => order.Warehouse).WithMany().HasForeignKey(order => order.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<SalesOrderLine>(entity =>
        {
            entity.Property(line => line.Quantity).HasPrecision(18, 3);
            entity.Property(line => line.UnitPrice).HasPrecision(18, 2);
            entity.Property(line => line.DiscountPercentage).HasPrecision(5, 2);
            entity.HasOne(line => line.SalesOrder).WithMany(order => order.Lines).HasForeignKey(line => line.SalesOrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(line => line.Product).WithMany(product => product.SalesOrderLines).HasForeignKey(line => line.ProductId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Supplier>(entity => entity.HasIndex(supplier => supplier.TaxId).IsUnique());
        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasIndex(order => order.Number).IsUnique();
            entity.HasOne(order => order.Supplier).WithMany(supplier => supplier.PurchaseOrders).HasForeignKey(order => order.SupplierId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(order => order.Warehouse).WithMany().HasForeignKey(order => order.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<PurchaseOrderLine>(entity =>
        {
            entity.Property(line => line.Quantity).HasPrecision(18, 3);
            entity.Property(line => line.UnitPrice).HasPrecision(18, 2);
            entity.Property(line => line.DiscountPercentage).HasPrecision(5, 2);
            entity.HasOne(line => line.PurchaseOrder).WithMany(order => order.Lines).HasForeignKey(line => line.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(line => line.Product).WithMany(product => product.PurchaseOrderLines).HasForeignKey(line => line.ProductId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasIndex(invoice => new { invoice.Series, invoice.Number }).IsUnique();
            entity.HasIndex(invoice => invoice.SalesOrderId).IsUnique().HasFilter("[SalesOrderId] IS NOT NULL");
            entity.HasIndex(invoice => invoice.PurchaseOrderId).IsUnique().HasFilter("[PurchaseOrderId] IS NOT NULL");
            entity.Property(invoice => invoice.TaxBase).HasPrecision(18, 2);
            entity.Property(invoice => invoice.TaxRate).HasPrecision(18, 2);
            entity.Property(invoice => invoice.TaxAmount).HasPrecision(18, 2);
            entity.Property(invoice => invoice.Total).HasPrecision(18, 2);
            entity.HasOne(invoice => invoice.SalesOrder).WithMany().HasForeignKey(invoice => invoice.SalesOrderId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(invoice => invoice.PurchaseOrder).WithMany().HasForeignKey(invoice => invoice.PurchaseOrderId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(payment => payment.Amount).HasPrecision(18, 2);
            entity.HasOne(payment => payment.Invoice).WithMany(invoice => invoice.Payments).HasForeignKey(payment => payment.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Warehouse>(entity => entity.HasIndex(warehouse => warehouse.Code).IsUnique());
        modelBuilder.Entity<WarehouseStock>(entity =>
        {
            entity.HasKey(stock => new { stock.WarehouseId, stock.ProductId });
            entity.Property(stock => stock.Quantity).HasPrecision(18, 3);
            entity.Property(stock => stock.RowVersion).IsRowVersion();
            entity.HasOne(stock => stock.Warehouse).WithMany(warehouse => warehouse.Stocks).HasForeignKey(stock => stock.WarehouseId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(stock => stock.Product).WithMany(product => product.WarehouseStocks).HasForeignKey(stock => stock.ProductId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.Property(movement => movement.Quantity).HasPrecision(18, 3);
            entity.HasIndex(movement => movement.CreatedAt);
            entity.HasOne(movement => movement.Product).WithMany(product => product.StockMovements).HasForeignKey(movement => movement.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(movement => movement.SourceWarehouse).WithMany(warehouse => warehouse.SourceMovements).HasForeignKey(movement => movement.SourceWarehouseId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(movement => movement.DestinationWarehouse).WithMany(warehouse => warehouse.DestinationMovements).HasForeignKey(movement => movement.DestinationWarehouseId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
