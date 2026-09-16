using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using MiniErp.Data;

namespace MiniErp.Migrations;

[DbContext(typeof(AppDbContext))]
public class AppDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.11").HasAnnotation("Relational:MaxIdentifierLength", 128);
        SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

        modelBuilder.Entity("MiniErp.Models.Customer", entity =>
        {
            entity.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").UseIdentityColumn();
            entity.Property<string>("Address").HasMaxLength(240).HasColumnType("nvarchar(240)");
            entity.Property<string>("Email").HasMaxLength(160).HasColumnType("nvarchar(160)");
            entity.Property<bool>("IsActive").HasColumnType("bit");
            entity.Property<string>("Name").IsRequired().HasMaxLength(120).HasColumnType("nvarchar(120)");
            entity.Property<string>("Phone").HasMaxLength(30).HasColumnType("nvarchar(30)");
            entity.Property<string>("TaxId").IsRequired().HasMaxLength(20).HasColumnType("nvarchar(20)");
            entity.HasKey("Id");
            entity.HasIndex("TaxId").IsUnique();
            entity.ToTable("Customers");
        });

        modelBuilder.Entity("MiniErp.Models.Supplier", entity =>
        {
            entity.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").UseIdentityColumn();
            entity.Property<string>("Address").HasMaxLength(240).HasColumnType("nvarchar(240)");
            entity.Property<string>("Email").HasMaxLength(160).HasColumnType("nvarchar(160)");
            entity.Property<bool>("IsActive").HasColumnType("bit");
            entity.Property<string>("Name").IsRequired().HasMaxLength(120).HasColumnType("nvarchar(120)");
            entity.Property<string>("Phone").HasMaxLength(30).HasColumnType("nvarchar(30)");
            entity.Property<string>("TaxId").IsRequired().HasMaxLength(20).HasColumnType("nvarchar(20)");
            entity.HasKey("Id"); entity.HasIndex("TaxId").IsUnique(); entity.ToTable("Suppliers");
        });

        modelBuilder.Entity("MiniErp.Models.Product", entity =>
        {
            entity.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").UseIdentityColumn();
            entity.Property<bool>("IsActive").HasColumnType("bit");
            entity.Property<string>("Name").IsRequired().HasMaxLength(160).HasColumnType("nvarchar(160)");
            entity.Property<string>("Sku").IsRequired().HasMaxLength(40).HasColumnType("nvarchar(40)");
            entity.Property<decimal>("Stock").HasPrecision(18, 3).HasColumnType("decimal(18,3)");
            entity.Property<decimal>("UnitPrice").HasPrecision(18, 2).HasColumnType("decimal(18,2)");
            entity.HasKey("Id");
            entity.HasIndex("Sku").IsUnique();
            entity.ToTable("Products");
        });

        modelBuilder.Entity("MiniErp.Models.PurchaseOrder", entity =>
        {
            entity.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").UseIdentityColumn();
            entity.Property<string>("Number").IsRequired().HasMaxLength(30).HasColumnType("nvarchar(30)");
            entity.Property<DateTime>("OrderDate").HasColumnType("datetime2"); entity.Property<int>("Status").HasColumnType("int"); entity.Property<int>("SupplierId").HasColumnType("int");
            entity.Property<int>("WarehouseId").HasColumnType("int");
            entity.HasKey("Id"); entity.HasIndex("Number").IsUnique(); entity.HasIndex("SupplierId"); entity.HasIndex("WarehouseId"); entity.ToTable("PurchaseOrders");
        });

        modelBuilder.Entity("MiniErp.Models.PurchaseOrderLine", entity =>
        {
            entity.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").UseIdentityColumn();
            entity.Property<decimal>("DiscountPercentage").HasPrecision(5, 2).HasColumnType("decimal(5,2)"); entity.Property<int>("ProductId").HasColumnType("int"); entity.Property<int>("PurchaseOrderId").HasColumnType("int"); entity.Property<decimal>("Quantity").HasPrecision(18, 3).HasColumnType("decimal(18,3)"); entity.Property<decimal>("UnitPrice").HasPrecision(18, 2).HasColumnType("decimal(18,2)");
            entity.HasKey("Id"); entity.HasIndex("ProductId"); entity.HasIndex("PurchaseOrderId"); entity.ToTable("PurchaseOrderLines");
        });

        modelBuilder.Entity("MiniErp.Models.Warehouse", entity =>
        {
            entity.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").UseIdentityColumn(); entity.Property<string>("Code").IsRequired().HasMaxLength(20).HasColumnType("nvarchar(20)"); entity.Property<bool>("IsActive").HasColumnType("bit"); entity.Property<string>("Name").IsRequired().HasMaxLength(120).HasColumnType("nvarchar(120)"); entity.HasKey("Id"); entity.HasIndex("Code").IsUnique(); entity.ToTable("Warehouses");
        });
        modelBuilder.Entity("MiniErp.Models.WarehouseStock", entity =>
        {
            entity.Property<int>("WarehouseId").HasColumnType("int"); entity.Property<int>("ProductId").HasColumnType("int"); entity.Property<decimal>("Quantity").HasPrecision(18,3).HasColumnType("decimal(18,3)"); entity.Property<byte[]>("RowVersion").IsConcurrencyToken().ValueGeneratedOnAddOrUpdate().HasColumnType("rowversion"); entity.HasKey("WarehouseId","ProductId"); entity.HasIndex("ProductId"); entity.ToTable("WarehouseStocks");
        });
        modelBuilder.Entity("MiniErp.Models.StockMovement", entity =>
        {
            entity.Property<long>("Id").ValueGeneratedOnAdd().HasColumnType("bigint").UseIdentityColumn(); entity.Property<DateTime>("CreatedAt").HasColumnType("datetime2"); entity.Property<int?>("DestinationWarehouseId").HasColumnType("int"); entity.Property<int>("ProductId").HasColumnType("int"); entity.Property<decimal>("Quantity").HasPrecision(18,3).HasColumnType("decimal(18,3)"); entity.Property<string>("Reference").IsRequired().HasMaxLength(160).HasColumnType("nvarchar(160)"); entity.Property<int?>("SourceWarehouseId").HasColumnType("int"); entity.Property<int>("Type").HasColumnType("int"); entity.HasKey("Id"); entity.HasIndex("CreatedAt"); entity.HasIndex("ProductId"); entity.HasIndex("SourceWarehouseId"); entity.HasIndex("DestinationWarehouseId"); entity.ToTable("StockMovements");
        });

        modelBuilder.Entity("MiniErp.Models.SalesOrder", entity =>
        {
            entity.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").UseIdentityColumn();
            entity.Property<int>("CustomerId").HasColumnType("int");
            entity.Property<int>("WarehouseId").HasColumnType("int");
            entity.Property<string>("Number").IsRequired().HasMaxLength(30).HasColumnType("nvarchar(30)");
            entity.Property<DateTime>("OrderDate").HasColumnType("datetime2");
            entity.Property<int>("Status").HasColumnType("int");
            entity.HasKey("Id");
            entity.HasIndex("CustomerId");
            entity.HasIndex("WarehouseId");
            entity.HasIndex("Number").IsUnique();
            entity.ToTable("SalesOrders");
        });

        modelBuilder.Entity("MiniErp.Models.SalesOrderLine", entity =>
        {
            entity.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").UseIdentityColumn();
            entity.Property<decimal>("DiscountPercentage").HasPrecision(5, 2).HasColumnType("decimal(5,2)");
            entity.Property<int>("ProductId").HasColumnType("int");
            entity.Property<decimal>("Quantity").HasPrecision(18, 3).HasColumnType("decimal(18,3)");
            entity.Property<int>("SalesOrderId").HasColumnType("int");
            entity.Property<decimal>("UnitPrice").HasPrecision(18, 2).HasColumnType("decimal(18,2)");
            entity.HasKey("Id");
            entity.HasIndex("ProductId");
            entity.HasIndex("SalesOrderId");
            entity.ToTable("SalesOrderLines");
        });

        modelBuilder.Entity("MiniErp.Models.PurchaseOrder", entity =>
        {
            entity.HasOne("MiniErp.Models.Supplier", "Supplier").WithMany("PurchaseOrders").HasForeignKey("SupplierId").OnDelete(DeleteBehavior.Restrict).IsRequired();
            entity.HasOne("MiniErp.Models.Warehouse", "Warehouse").WithMany().HasForeignKey("WarehouseId").OnDelete(DeleteBehavior.Restrict).IsRequired();
        });
        modelBuilder.Entity("MiniErp.Models.PurchaseOrderLine", entity =>
        {
            entity.HasOne("MiniErp.Models.Product", "Product").WithMany("PurchaseOrderLines").HasForeignKey("ProductId").OnDelete(DeleteBehavior.Restrict).IsRequired();
            entity.HasOne("MiniErp.Models.PurchaseOrder", "PurchaseOrder").WithMany("Lines").HasForeignKey("PurchaseOrderId").OnDelete(DeleteBehavior.Cascade).IsRequired();
        });

        modelBuilder.Entity("MiniErp.Models.WarehouseStock", entity => { entity.HasOne("MiniErp.Models.Product","Product").WithMany("WarehouseStocks").HasForeignKey("ProductId").OnDelete(DeleteBehavior.Restrict).IsRequired(); entity.HasOne("MiniErp.Models.Warehouse","Warehouse").WithMany("Stocks").HasForeignKey("WarehouseId").OnDelete(DeleteBehavior.Restrict).IsRequired(); });
        modelBuilder.Entity("MiniErp.Models.StockMovement", entity => { entity.HasOne("MiniErp.Models.Product","Product").WithMany("StockMovements").HasForeignKey("ProductId").OnDelete(DeleteBehavior.Restrict).IsRequired(); entity.HasOne("MiniErp.Models.Warehouse","SourceWarehouse").WithMany("SourceMovements").HasForeignKey("SourceWarehouseId").OnDelete(DeleteBehavior.Restrict); entity.HasOne("MiniErp.Models.Warehouse","DestinationWarehouse").WithMany("DestinationMovements").HasForeignKey("DestinationWarehouseId").OnDelete(DeleteBehavior.Restrict); });
        modelBuilder.Entity("MiniErp.Models.SalesOrder", entity =>
        {
            entity.HasOne("MiniErp.Models.Customer", "Customer").WithMany("SalesOrders").HasForeignKey("CustomerId").OnDelete(DeleteBehavior.Restrict).IsRequired();
            entity.HasOne("MiniErp.Models.Warehouse", "Warehouse").WithMany().HasForeignKey("WarehouseId").OnDelete(DeleteBehavior.Restrict).IsRequired();
        });

        modelBuilder.Entity("MiniErp.Models.SalesOrderLine", entity =>
        {
            entity.HasOne("MiniErp.Models.Product", "Product").WithMany("SalesOrderLines").HasForeignKey("ProductId").OnDelete(DeleteBehavior.Restrict).IsRequired();
            entity.HasOne("MiniErp.Models.SalesOrder", "SalesOrder").WithMany("Lines").HasForeignKey("SalesOrderId").OnDelete(DeleteBehavior.Cascade).IsRequired();
        });
    }
}
