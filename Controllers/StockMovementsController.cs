using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniErp.Data;
using MiniErp.Models;
using MiniErp.ViewModels;

namespace MiniErp.Controllers;

public class StockMovementsController(AppDbContext db) : Controller
{
    [HttpGet]
    public IActionResult Index(int? productId, int? warehouseId)
    {
        var query = db.StockMovements.AsNoTracking()
            .Include(movement => movement.Product)
            .Include(movement => movement.SourceWarehouse)
            .Include(movement => movement.DestinationWarehouse)
            .AsQueryable();

        if (productId.HasValue)
            query = query.Where(movement => movement.ProductId == productId);
        if (warehouseId.HasValue)
            query = query.Where(movement => movement.SourceWarehouseId == warehouseId || movement.DestinationWarehouseId == warehouseId);

        var movements = query.OrderByDescending(movement => movement.CreatedAt).Take(500).ToArray();
        var references = movements.Select(movement => movement.Reference).Distinct().ToArray();

        ViewBag.PurchaseOrderReferences = db.PurchaseOrders.AsNoTracking()
            .Where(order => references.Contains(order.Number))
            .Select(order => order.Number)
            .ToHashSet();
        ViewBag.SalesOrderReferences = db.SalesOrders.AsNoTracking()
            .Where(order => references.Contains(order.Number))
            .Select(order => order.Number)
            .ToHashSet();

        Lists();
        return View(movements);
    }

    [HttpGet]
    public IActionResult ManualEntry()
    {
        Lists();
        return View("Create", new StockMovementViewModel
        {
            Type = StockMovementType.Entry,
            Reference = "Entrada manual"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ManualEntry(StockMovementViewModel model)
    {
        model.Type = StockMovementType.Entry;
        model.SourceWarehouseId = null;
        ModelState.Remove(nameof(model.Type));
        ModelState.Remove(nameof(model.SourceWarehouseId));
        return SaveMovement(model, "Create");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(StockMovementViewModel model) => SaveMovement(model, "Create");

    private IActionResult SaveMovement(StockMovementViewModel model, string viewName)
    {
        ValidateReferences(model);
        if (!ModelState.IsValid)
        {
            Lists();
            return View(viewName, model);
        }

        using var transaction = db.Database.IsRelational() ? db.Database.BeginTransaction() : null;
        var product = db.Products.Find(model.ProductId)!;

        if (model.Type is StockMovementType.Exit or StockMovementType.Transfer)
        {
            var source = Stock(model.SourceWarehouseId!.Value, model.ProductId);
            if (source.Quantity < model.Quantity)
            {
                ModelState.AddModelError(nameof(model.Quantity), "Stock insuficiente.");
                Lists();
                return View(viewName, model);
            }

            source.Quantity -= model.Quantity;
            product.Stock -= model.Quantity;
        }

        if (model.Type is StockMovementType.Entry or StockMovementType.Transfer)
        {
            var destination = Stock(model.DestinationWarehouseId!.Value, model.ProductId);
            destination.Quantity += model.Quantity;
            product.Stock += model.Quantity;
        }

        if (model.Type == StockMovementType.Adjustment)
        {
            var destination = Stock(model.DestinationWarehouseId!.Value, model.ProductId);
            var difference = model.Quantity - destination.Quantity;
            destination.Quantity = model.Quantity;
            product.Stock += difference;
        }

        db.StockMovements.Add(new StockMovement
        {
            Type = model.Type,
            ProductId = model.ProductId,
            SourceWarehouseId = model.SourceWarehouseId,
            DestinationWarehouseId = model.DestinationWarehouseId,
            Quantity = model.Quantity,
            Reference = model.Reference.Trim()
        });

        db.SaveChanges();
        transaction?.Commit();
        return RedirectToAction(nameof(Index));
    }

    private WarehouseStock Stock(int warehouseId, int productId)
    {
        var stock = db.WarehouseStocks.Find(warehouseId, productId);
        if (stock is not null)
            return stock;

        stock = new WarehouseStock { WarehouseId = warehouseId, ProductId = productId };
        db.Add(stock);
        return stock;
    }

    private void ValidateReferences(StockMovementViewModel model)
    {
        if (!db.Products.Any(product => product.Id == model.ProductId && product.IsActive))
            ModelState.AddModelError(nameof(model.ProductId), "Artículo no válido.");

        var warehouseIds = new[] { model.SourceWarehouseId, model.DestinationWarehouseId }
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToArray();

        if (db.Warehouses.Count(warehouse => warehouseIds.Contains(warehouse.Id) && warehouse.IsActive) != warehouseIds.Length)
            ModelState.AddModelError(string.Empty, "Almacén no válido.");
    }

    private void Lists()
    {
        ViewBag.Products = new SelectList(db.Products.AsNoTracking().Where(product => product.IsActive).OrderBy(product => product.Name), "Id", "Name");
        ViewBag.Warehouses = new SelectList(db.Warehouses.AsNoTracking().Where(warehouse => warehouse.IsActive).OrderBy(warehouse => warehouse.Name), "Id", "Name");
    }
}
