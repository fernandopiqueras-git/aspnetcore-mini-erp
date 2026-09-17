using MiniErp.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniErp.Data;
using MiniErp.Models;

namespace MiniErp.Controllers;

[Authorize(Roles = AppRoles.Administrator + "," + AppRoles.Sales + "," + AppRoles.Purchasing + "," + AppRoles.Warehouse)]
public class ProductsController(AppDbContext database) : Controller
{
    [HttpGet]
    public IActionResult Index(string? search, bool? active)
    {
        var query = database.Products.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(product => product.Name.Contains(term) || product.Sku.Contains(term));
        }
        if (active.HasValue)
            query = query.Where(product => product.IsActive == active);

        ViewBag.Search = search;
        ViewBag.Active = active;
        return View(query.OrderBy(product => product.Name).ToArray());
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var product = database.Products.AsNoTracking().Include(item => item.SalesOrderLines).FirstOrDefault(item => item.Id == id);
        return product is null ? NotFound() : View(product);
    }

    [HttpGet]
    public IActionResult Create() => View("Form", new Product());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Product product)
    {
        Normalize(product);
        ValidateDuplicateSku(product);
        if (!ModelState.IsValid)
            return View("Form", product);

        product.Stock = 0;
        database.Products.Add(product);
        database.SaveChanges();
        return RedirectToAction(nameof(Details), new { id = product.Id });
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var product = database.Products.AsNoTracking().FirstOrDefault(item => item.Id == id);
        return product is null ? NotFound() : View("Form", product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Product product)
    {
        if (id != product.Id)
            return BadRequest();
        var stored = database.Products.FirstOrDefault(item => item.Id == id);
        if (stored is null)
            return NotFound();

        Normalize(product);
        ValidateDuplicateSku(product);
        if (!ModelState.IsValid)
            return View("Form", product);

        stored.Sku = product.Sku;
        stored.Name = product.Name;
        stored.UnitPrice = product.UnitPrice;
        stored.IsActive = product.IsActive;
        database.SaveChanges();
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        var product = database.Products.AsNoTracking().FirstOrDefault(item => item.Id == id);
        return product is null ? NotFound() : View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var product = database.Products.Find(id);
        if (product is null)
            return NotFound();
        if (database.SalesOrderLines.Any(line => line.ProductId == id)
            || database.PurchaseOrderLines.Any(line => line.ProductId == id)
            || database.WarehouseStocks.Any(stock => stock.ProductId == id)
            || database.StockMovements.Any(movement => movement.ProductId == id))
        {
            TempData["Error"] = "No se puede eliminar un artículo con pedidos, existencias o movimientos de stock.";
            return RedirectToAction(nameof(Details), new { id });
        }

        database.Products.Remove(product);
        database.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

    private void ValidateDuplicateSku(Product product)
    {
        if (database.Products.Any(item => item.Id != product.Id && item.Sku == product.Sku))
            ModelState.AddModelError(nameof(Product.Sku), "Ya existe un artículo con este SKU.");
    }

    private static void Normalize(Product product)
    {
        product.Sku = product.Sku?.Trim().ToUpperInvariant() ?? string.Empty;
        product.Name = product.Name?.Trim() ?? string.Empty;
    }
}
