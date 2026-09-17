using MiniErp.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniErp.Data;
using MiniErp.Models;
namespace MiniErp.Controllers;
[Authorize(Roles = AppRoles.Administrator + "," + AppRoles.Purchasing)]
public class SuppliersController(AppDbContext db) : Controller
{
 [HttpGet] public IActionResult Index(string? search, bool? active) { var q=db.Suppliers.AsNoTracking().AsQueryable(); if(!string.IsNullOrWhiteSpace(search)){var t=search.Trim();q=q.Where(x=>x.Name.Contains(t)||x.TaxId.Contains(t));} if(active.HasValue)q=q.Where(x=>x.IsActive==active);ViewBag.Search=search;ViewBag.Active=active;return View(q.OrderBy(x=>x.Name).ToArray()); }
 [HttpGet] public IActionResult Details(int id){var x=db.Suppliers.AsNoTracking().Include(x=>x.PurchaseOrders).FirstOrDefault(x=>x.Id==id);return x is null?NotFound():View(x);}
 [HttpGet] public IActionResult Create()=>View("Form",new Supplier());
 [HttpPost,ValidateAntiForgeryToken] public IActionResult Create(Supplier x){Normalize(x);Duplicate(x);if(!ModelState.IsValid)return View("Form",x);db.Add(x);db.SaveChanges();return RedirectToAction(nameof(Details),new{id=x.Id});}
 [HttpGet] public IActionResult Edit(int id){var x=db.Suppliers.AsNoTracking().FirstOrDefault(x=>x.Id==id);return x is null?NotFound():View("Form",x);}
 [HttpPost,ValidateAntiForgeryToken] public IActionResult Edit(int id,Supplier x){if(id!=x.Id)return BadRequest();var e=db.Suppliers.Find(id);if(e is null)return NotFound();Normalize(x);Duplicate(x);if(!ModelState.IsValid)return View("Form",x);e.Name=x.Name;e.TaxId=x.TaxId;e.Email=x.Email;e.Phone=x.Phone;e.Address=x.Address;e.IsActive=x.IsActive;db.SaveChanges();return RedirectToAction(nameof(Details),new{id});}
 [HttpGet] public IActionResult Delete(int id){var x=db.Suppliers.AsNoTracking().FirstOrDefault(x=>x.Id==id);return x is null?NotFound():View(x);}
 [HttpPost,ActionName("Delete"),ValidateAntiForgeryToken] public IActionResult DeleteConfirmed(int id){var x=db.Suppliers.Find(id);if(x is null)return NotFound();if(db.PurchaseOrders.Any(o=>o.SupplierId==id)){if(ControllerContext.HttpContext?.RequestServices.GetService(typeof(Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory)) is not null)TempData["Error"]="No se puede eliminar el proveedor porque tiene pedidos de compra. Puedes desactivarlo desde Editar.";return RedirectToAction(nameof(Details),new{id});}db.Remove(x);db.SaveChanges();return RedirectToAction(nameof(Index));}
 private void Duplicate(Supplier x){if(db.Suppliers.Any(y=>y.Id!=x.Id&&y.TaxId==x.TaxId))ModelState.AddModelError(nameof(x.TaxId),"Ya existe un proveedor con este NIF o CIF.");}
 private static void Normalize(Supplier x){x.Name=x.Name?.Trim()??"";x.TaxId=string.Concat((x.TaxId??"").Where(char.IsLetterOrDigit)).ToUpperInvariant();x.Email=EmptyToNull(x.Email)?.ToLowerInvariant();x.Phone=EmptyToNull(x.Phone);x.Address=EmptyToNull(x.Address);}
 private static string? EmptyToNull(string? value)=>string.IsNullOrWhiteSpace(value)?null:value.Trim();
}