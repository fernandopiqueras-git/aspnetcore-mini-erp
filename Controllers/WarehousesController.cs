using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniErp.Data;
using MiniErp.Models;
namespace MiniErp.Controllers;
public class WarehousesController(AppDbContext db):Controller
{
 [HttpGet] public IActionResult Index()=>View(db.Warehouses.AsNoTracking().Include(x=>x.Stocks).OrderBy(x=>x.Code).ToArray());
 [HttpGet] public IActionResult Details(int id){var x=db.Warehouses.AsNoTracking().Include(x=>x.Stocks).ThenInclude(x=>x.Product).FirstOrDefault(x=>x.Id==id);return x is null?NotFound():View(x);}
 [HttpGet] public IActionResult Create()=>View("Form",new Warehouse());
 [HttpPost,ValidateAntiForgeryToken] public IActionResult Create(Warehouse x){Normalize(x);Duplicate(x);if(!ModelState.IsValid)return View("Form",x);db.Add(x);db.SaveChanges();return RedirectToAction(nameof(Details),new{id=x.Id});}
 [HttpGet] public IActionResult Edit(int id){var x=db.Warehouses.AsNoTracking().FirstOrDefault(x=>x.Id==id);return x is null?NotFound():View("Form",x);}
 [HttpPost,ValidateAntiForgeryToken] public IActionResult Edit(int id,Warehouse x){if(id!=x.Id)return BadRequest();var e=db.Warehouses.Find(id);if(e is null)return NotFound();Normalize(x);Duplicate(x);if(!ModelState.IsValid)return View("Form",x);e.Code=x.Code;e.Name=x.Name;e.IsActive=x.IsActive;db.SaveChanges();return RedirectToAction(nameof(Details),new{id});}
 [HttpPost,ValidateAntiForgeryToken] public IActionResult Disable(int id){var x=db.Warehouses.Find(id);if(x is null)return NotFound();if(db.WarehouseStocks.Any(s=>s.WarehouseId==id&&s.Quantity!=0))return RedirectToAction(nameof(Details),new{id});x.IsActive=false;db.SaveChanges();return RedirectToAction(nameof(Details),new{id});}
 private void Duplicate(Warehouse x){if(db.Warehouses.Any(y=>y.Id!=x.Id&&y.Code==x.Code))ModelState.AddModelError(nameof(x.Code),"El código ya existe.");}
 private static void Normalize(Warehouse x){x.Code=x.Code?.Trim().ToUpperInvariant()??"";x.Name=x.Name?.Trim()??"";}
}