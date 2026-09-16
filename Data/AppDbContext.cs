using Microsoft.EntityFrameworkCore;
using MiniErp.Models;
namespace MiniErp.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
 public DbSet<Customer> Customers=>Set<Customer>(); public DbSet<Product> Products=>Set<Product>(); public DbSet<SalesOrder> SalesOrders=>Set<SalesOrder>(); public DbSet<SalesOrderLine> SalesOrderLines=>Set<SalesOrderLine>();
 public DbSet<Supplier> Suppliers=>Set<Supplier>(); public DbSet<PurchaseOrder> PurchaseOrders=>Set<PurchaseOrder>(); public DbSet<PurchaseOrderLine> PurchaseOrderLines=>Set<PurchaseOrderLine>();
 public DbSet<Warehouse> Warehouses=>Set<Warehouse>(); public DbSet<WarehouseStock> WarehouseStocks=>Set<WarehouseStock>(); public DbSet<StockMovement> StockMovements=>Set<StockMovement>();
 protected override void OnModelCreating(ModelBuilder b)
 {
  b.Entity<Customer>(e=>{e.HasIndex(x=>x.TaxId).IsUnique();e.Property(x=>x.Name).IsUnicode();});
  b.Entity<Product>(e=>{e.HasIndex(x=>x.Sku).IsUnique();e.Property(x=>x.UnitPrice).HasPrecision(18,2);e.Property(x=>x.Stock).HasPrecision(18,3);});
  b.Entity<SalesOrder>(e=>{e.HasIndex(x=>x.Number).IsUnique();e.HasOne(x=>x.Customer).WithMany(x=>x.SalesOrders).HasForeignKey(x=>x.CustomerId).OnDelete(DeleteBehavior.Restrict);e.HasOne(x=>x.Warehouse).WithMany().HasForeignKey(x=>x.WarehouseId).OnDelete(DeleteBehavior.Restrict);});
  b.Entity<SalesOrderLine>(e=>{e.Property(x=>x.Quantity).HasPrecision(18,3);e.Property(x=>x.UnitPrice).HasPrecision(18,2);e.Property(x=>x.DiscountPercentage).HasPrecision(5,2);e.HasOne(x=>x.SalesOrder).WithMany(x=>x.Lines).HasForeignKey(x=>x.SalesOrderId).OnDelete(DeleteBehavior.Cascade);e.HasOne(x=>x.Product).WithMany(x=>x.SalesOrderLines).HasForeignKey(x=>x.ProductId).OnDelete(DeleteBehavior.Restrict);});
  b.Entity<Supplier>(e=>e.HasIndex(x=>x.TaxId).IsUnique());
  b.Entity<PurchaseOrder>(e=>{e.HasIndex(x=>x.Number).IsUnique();e.HasOne(x=>x.Supplier).WithMany(x=>x.PurchaseOrders).HasForeignKey(x=>x.SupplierId).OnDelete(DeleteBehavior.Restrict);e.HasOne(x=>x.Warehouse).WithMany().HasForeignKey(x=>x.WarehouseId).OnDelete(DeleteBehavior.Restrict);});
  b.Entity<PurchaseOrderLine>(e=>{e.Property(x=>x.Quantity).HasPrecision(18,3);e.Property(x=>x.UnitPrice).HasPrecision(18,2);e.Property(x=>x.DiscountPercentage).HasPrecision(5,2);e.HasOne(x=>x.PurchaseOrder).WithMany(x=>x.Lines).HasForeignKey(x=>x.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade);e.HasOne(x=>x.Product).WithMany(x=>x.PurchaseOrderLines).HasForeignKey(x=>x.ProductId).OnDelete(DeleteBehavior.Restrict);});
  b.Entity<Warehouse>(e=>e.HasIndex(x=>x.Code).IsUnique());
  b.Entity<WarehouseStock>(e=>{e.HasKey(x=>new{x.WarehouseId,x.ProductId});e.Property(x=>x.Quantity).HasPrecision(18,3);e.Property(x=>x.RowVersion).IsRowVersion();e.HasOne(x=>x.Warehouse).WithMany(x=>x.Stocks).HasForeignKey(x=>x.WarehouseId).OnDelete(DeleteBehavior.Restrict);e.HasOne(x=>x.Product).WithMany(x=>x.WarehouseStocks).HasForeignKey(x=>x.ProductId).OnDelete(DeleteBehavior.Restrict);});
  b.Entity<StockMovement>(e=>{e.Property(x=>x.Quantity).HasPrecision(18,3);e.HasIndex(x=>x.CreatedAt);e.HasOne(x=>x.Product).WithMany(x=>x.StockMovements).HasForeignKey(x=>x.ProductId).OnDelete(DeleteBehavior.Restrict);e.HasOne(x=>x.SourceWarehouse).WithMany(x=>x.SourceMovements).HasForeignKey(x=>x.SourceWarehouseId).OnDelete(DeleteBehavior.Restrict);e.HasOne(x=>x.DestinationWarehouse).WithMany(x=>x.DestinationMovements).HasForeignKey(x=>x.DestinationWarehouseId).OnDelete(DeleteBehavior.Restrict);});
 }
}