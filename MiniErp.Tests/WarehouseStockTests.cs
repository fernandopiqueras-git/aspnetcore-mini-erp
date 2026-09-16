using Microsoft.EntityFrameworkCore;
using MiniErp.Controllers;
using MiniErp.Data;
using MiniErp.Models;
using MiniErp.ViewModels;
namespace MiniErp.Tests;
public class WarehouseStockTests
{
 [Fact] public void Transfer_PreservesTotalStock(){using var db=Create();Seed(db);var c=new StockMovementsController(db);c.Create(new(){Type=StockMovementType.Transfer,ProductId=1,SourceWarehouseId=1,DestinationWarehouseId=2,Quantity=3,Reference="TR"});Assert.Equal(10m,db.Products.Single().Stock);Assert.Equal(7m,db.WarehouseStocks.Find(1,1)!.Quantity);Assert.Equal(3m,db.WarehouseStocks.Find(2,1)!.Quantity);}
 [Fact] public void Exit_RejectsNegativeStock(){using var db=Create();Seed(db);var c=new StockMovementsController(db);c.Create(new(){Type=StockMovementType.Exit,ProductId=1,SourceWarehouseId=1,Quantity=11,Reference="OUT"});Assert.Equal(10m,db.WarehouseStocks.Find(1,1)!.Quantity);}
 [Fact] public void Adjustment_UpdatesAggregate(){using var db=Create();Seed(db);new StockMovementsController(db).Create(new(){Type=StockMovementType.Adjustment,ProductId=1,DestinationWarehouseId=1,Quantity=6,Reference="COUNT"});Assert.Equal(6m,db.Products.Single().Stock);}
 [Fact] public void WarehouseCode_IsUnique(){using var db=Create();Seed(db);var c=new WarehousesController(db);var result=c.Create(new(){Code=" main ",Name="Otro"});Assert.False(c.ModelState.IsValid);}
 private static AppDbContext Create()=>new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
 private static void Seed(AppDbContext db){db.AddRange(new Warehouse{Id=1,Code="MAIN",Name="Principal"},new Warehouse{Id=2,Code="SECOND",Name="Secundario"},new Product{Id=1,Sku="A",Name="Artículo",Stock=10,IsActive=true},new WarehouseStock{WarehouseId=1,ProductId=1,Quantity=10});db.SaveChanges();}
}