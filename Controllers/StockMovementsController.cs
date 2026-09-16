using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniErp.Data;
using MiniErp.Models;
using MiniErp.ViewModels;
namespace MiniErp.Controllers;
public class StockMovementsController(AppDbContext db):Controller
{
 [HttpGet] public IActionResult Index(int? productId,int? warehouseId){var q=db.StockMovements.AsNoTracking().Include(x=>x.Product).Include(x=>x.SourceWarehouse).Include(x=>x.DestinationWarehouse).AsQueryable();if(productId.HasValue)q=q.Where(x=>x.ProductId==productId);if(warehouseId.HasValue)q=q.Where(x=>x.SourceWarehouseId==warehouseId||x.DestinationWarehouseId==warehouseId);Lists();return View(q.OrderByDescending(x=>x.CreatedAt).Take(500).ToArray());}
 [HttpGet] public IActionResult Create(){Lists();return View(new StockMovementViewModel());}
 [HttpPost,ValidateAntiForgeryToken] public IActionResult Create(StockMovementViewModel m)
 {
  ValidateReferences(m);if(!ModelState.IsValid){Lists();return View(m);}
  using var tr=db.Database.IsRelational()?db.Database.BeginTransaction():null;
  var product=db.Products.Find(m.ProductId)!;
  if(m.Type is StockMovementType.Exit or StockMovementType.Transfer){var source=Stock(m.SourceWarehouseId!.Value,m.ProductId);if(source.Quantity<m.Quantity){ModelState.AddModelError(nameof(m.Quantity),"Stock insuficiente.");Lists();return View(m);}source.Quantity-=m.Quantity;product.Stock-=m.Quantity;}
  if(m.Type is StockMovementType.Entry or StockMovementType.Transfer){var destination=Stock(m.DestinationWarehouseId!.Value,m.ProductId);destination.Quantity+=m.Quantity;product.Stock+=m.Quantity;}
  if(m.Type==StockMovementType.Adjustment){var destination=Stock(m.DestinationWarehouseId!.Value,m.ProductId);var difference=m.Quantity-destination.Quantity;destination.Quantity=m.Quantity;product.Stock+=difference;}
    db.StockMovements.Add(new(){Type=m.Type,ProductId=m.ProductId,SourceWarehouseId=m.SourceWarehouseId,DestinationWarehouseId=m.DestinationWarehouseId,Quantity=m.Quantity,Reference=m.Reference.Trim()});
  db.SaveChanges();tr?.Commit();return RedirectToAction(nameof(Index));
 }
 private WarehouseStock Stock(int warehouseId,int productId){var x=db.WarehouseStocks.Find(warehouseId,productId);if(x is not null)return x;x=new(){WarehouseId=warehouseId,ProductId=productId};db.Add(x);return x;}
 private void ValidateReferences(StockMovementViewModel m){if(!db.Products.Any(x=>x.Id==m.ProductId&&x.IsActive))ModelState.AddModelError(nameof(m.ProductId),"Artículo no válido.");var ids=new[]{m.SourceWarehouseId,m.DestinationWarehouseId}.Where(x=>x.HasValue).Select(x=>x!.Value).Distinct().ToArray();if(db.Warehouses.Count(x=>ids.Contains(x.Id)&&x.IsActive)!=ids.Length)ModelState.AddModelError("", "Almacén no válido.");}
 private void Lists(){ViewBag.Products=new SelectList(db.Products.AsNoTracking().Where(x=>x.IsActive).OrderBy(x=>x.Name),"Id","Name");ViewBag.Warehouses=new SelectList(db.Warehouses.AsNoTracking().Where(x=>x.IsActive).OrderBy(x=>x.Name),"Id","Name");}
}