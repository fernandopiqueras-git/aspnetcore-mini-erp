using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniErp.Controllers;
using MiniErp.Data;
using MiniErp.Models;
using MiniErp.ViewModels;
namespace MiniErp.Tests;
public class PurchasingTests
{
 [Fact] public void PurchaseLine_CalculatesDiscount(){var x=new PurchaseOrderLine{Quantity=4,UnitPrice=25,DiscountPercentage=10};Assert.Equal(90m,x.LineTotal);}
 [Fact] public void Receive_IncreasesStockOnlyOnce(){using var db=Database();Seed(db);var o=new PurchaseOrder{Number="PC-1",SupplierId=1,WarehouseId=1,Status=PurchaseOrderStatus.Confirmed,Lines=[new(){ProductId=1,Quantity=5,UnitPrice=2}]};db.Add(o);db.SaveChanges();var c=new PurchaseOrdersController(db);c.Receive(o.Id);c.Receive(o.Id);Assert.Equal(15m,db.Products.Single().Stock);Assert.Equal(PurchaseOrderStatus.Received,o.Status);}
 [Theory][InlineData(PurchaseOrderStatus.Received)][InlineData(PurchaseOrderStatus.Cancelled)] public void Edit_BlocksFinalOrders(PurchaseOrderStatus status){using var db=Database();Seed(db);var o=new PurchaseOrder{Number="PC-1",SupplierId=1,Status=status};db.Add(o);db.SaveChanges();var result=new PurchaseOrdersController(db).Edit(o.Id,new(){Id=o.Id,Number="CHANGED",SupplierId=1,Lines=[]});Assert.IsType<RedirectToActionResult>(result);Assert.Equal("PC-1",o.Number);}
 [Fact] public void Form_RejectsRepeatedProducts(){var m=new PurchaseOrderFormViewModel{Lines=[new(){ProductId=1},new(){ProductId=1}]};Assert.NotEmpty(m.Validate(new(m)));}
 [Fact] public void Supplier_DeleteIsBlockedWhenUsed(){using var db=Database();Seed(db);db.Add(new PurchaseOrder{Number="PC-1",SupplierId=1,WarehouseId=1});db.SaveChanges();var result=new SuppliersController(db).DeleteConfirmed(1);Assert.IsType<RedirectToActionResult>(result);Assert.Single(db.Suppliers);}
 private static AppDbContext Database()=>new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
 private static void Seed(AppDbContext db){db.Add(new Supplier{Id=1,Name="Proveedor",TaxId="B45000007"});db.Add(new Warehouse{Id=1,Code="MAIN",Name="Principal"});db.Add(new Product{Id=1,Sku="A",Name="Artículo",Stock=10,IsActive=true});db.Add(new WarehouseStock{WarehouseId=1,ProductId=1,Quantity=10});db.SaveChanges();}
}