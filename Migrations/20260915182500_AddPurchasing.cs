using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MiniErp.Data;
namespace MiniErp.Migrations;
[DbContext(typeof(AppDbContext))]
[Migration("20260915182500_AddPurchasing")]
public class AddPurchasing : Migration
{
 protected override void Up(MigrationBuilder m)
 {
  m.CreateTable("Suppliers",t=>new{Id=t.Column<int>("int",nullable:false).Annotation("SqlServer:Identity","1, 1"),Name=t.Column<string>("nvarchar(120)",maxLength:120,nullable:false),TaxId=t.Column<string>("nvarchar(20)",maxLength:20,nullable:false),Email=t.Column<string>("nvarchar(160)",maxLength:160,nullable:true),Phone=t.Column<string>("nvarchar(30)",maxLength:30,nullable:true),Address=t.Column<string>("nvarchar(240)",maxLength:240,nullable:true),IsActive=t.Column<bool>("bit",nullable:false)},constraints:t=>t.PrimaryKey("PK_Suppliers",x=>x.Id));
  m.CreateTable("PurchaseOrders",t=>new{Id=t.Column<int>("int",nullable:false).Annotation("SqlServer:Identity","1, 1"),Number=t.Column<string>("nvarchar(30)",maxLength:30,nullable:false),OrderDate=t.Column<DateTime>("datetime2",nullable:false),Status=t.Column<int>("int",nullable:false),SupplierId=t.Column<int>("int",nullable:false)},constraints:t=>{t.PrimaryKey("PK_PurchaseOrders",x=>x.Id);t.ForeignKey("FK_PurchaseOrders_Suppliers_SupplierId",x=>x.SupplierId,"Suppliers","Id",onDelete:ReferentialAction.Restrict);});
  m.CreateTable("PurchaseOrderLines",t=>new{Id=t.Column<int>("int",nullable:false).Annotation("SqlServer:Identity","1, 1"),PurchaseOrderId=t.Column<int>("int",nullable:false),ProductId=t.Column<int>("int",nullable:false),Quantity=t.Column<decimal>("decimal(18,3)",precision:18,scale:3,nullable:false),UnitPrice=t.Column<decimal>("decimal(18,2)",precision:18,scale:2,nullable:false),DiscountPercentage=t.Column<decimal>("decimal(5,2)",precision:5,scale:2,nullable:false)},constraints:t=>{t.PrimaryKey("PK_PurchaseOrderLines",x=>x.Id);t.ForeignKey("FK_PurchaseOrderLines_Products_ProductId",x=>x.ProductId,"Products","Id",onDelete:ReferentialAction.Restrict);t.ForeignKey("FK_PurchaseOrderLines_PurchaseOrders_PurchaseOrderId",x=>x.PurchaseOrderId,"PurchaseOrders","Id",onDelete:ReferentialAction.Cascade);});
  m.CreateIndex("IX_Suppliers_TaxId","Suppliers","TaxId",unique:true);m.CreateIndex("IX_PurchaseOrders_Number","PurchaseOrders","Number",unique:true);m.CreateIndex("IX_PurchaseOrders_SupplierId","PurchaseOrders","SupplierId");m.CreateIndex("IX_PurchaseOrderLines_ProductId","PurchaseOrderLines","ProductId");m.CreateIndex("IX_PurchaseOrderLines_PurchaseOrderId","PurchaseOrderLines","PurchaseOrderId");
 }
 protected override void Down(MigrationBuilder m){m.DropTable("PurchaseOrderLines");m.DropTable("PurchaseOrders");m.DropTable("Suppliers");}
}