using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MiniErp.Data;

namespace MiniErp.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260915132300_InitialCreate")]
public class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Customers",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                TaxId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Email = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                Address = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Customers", item => item.Id));

        migrationBuilder.CreateTable(
            name: "Products",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                Sku = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Stock = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Products", item => item.Id));

        migrationBuilder.CreateTable(
            name: "SalesOrders",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                Number = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                CustomerId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SalesOrders", item => item.Id);
                table.ForeignKey("FK_SalesOrders_Customers_CustomerId", item => item.CustomerId, "Customers", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "SalesOrderLines",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                SalesOrderId = table.Column<int>(type: "int", nullable: false),
                ProductId = table.Column<int>(type: "int", nullable: false),
                Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SalesOrderLines", item => item.Id);
                table.ForeignKey("FK_SalesOrderLines_Products_ProductId", item => item.ProductId, "Products", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_SalesOrderLines_SalesOrders_SalesOrderId", item => item.SalesOrderId, "SalesOrders", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_Customers_TaxId", "Customers", "TaxId", unique: true);
        migrationBuilder.CreateIndex("IX_Products_Sku", "Products", "Sku", unique: true);
        migrationBuilder.CreateIndex("IX_SalesOrderLines_ProductId", "SalesOrderLines", "ProductId");
        migrationBuilder.CreateIndex("IX_SalesOrderLines_SalesOrderId", "SalesOrderLines", "SalesOrderId");
        migrationBuilder.CreateIndex("IX_SalesOrders_CustomerId", "SalesOrders", "CustomerId");
        migrationBuilder.CreateIndex("IX_SalesOrders_Number", "SalesOrders", "Number", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("SalesOrderLines");
        migrationBuilder.DropTable("Products");
        migrationBuilder.DropTable("SalesOrders");
        migrationBuilder.DropTable("Customers");
    }
}
