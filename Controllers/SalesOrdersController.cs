using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniErp.Data;
using MiniErp.Models;
using MiniErp.ViewModels;

namespace MiniErp.Controllers;

public class SalesOrdersController(AppDbContext database) : Controller
{
    [HttpGet]
    public IActionResult Index(string? search, SalesOrderStatus? status)
    {
        var query = database.SalesOrders.AsNoTracking().Include(order => order.Customer).Include(order => order.Lines).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(order => order.Number.Contains(term) || order.Customer.Name.Contains(term));
        }
        if (status.HasValue)
            query = query.Where(order => order.Status == status);

        ViewBag.Search = search;
        ViewBag.Status = status;
        return View(query.OrderByDescending(order => order.OrderDate).ThenByDescending(order => order.Id).ToArray());
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var order = database.SalesOrders.AsNoTracking()
            .Include(item => item.Customer).Include(item => item.Warehouse)
            .Include(item => item.Lines).ThenInclude(line => line.Product)
            .FirstOrDefault(item => item.Id == id);
        return order is null ? NotFound() : View(order);
    }

    [HttpGet]
    public IActionResult Create()
    {
        LoadSelections();
        return View("Form", new SalesOrderFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(SalesOrderFormViewModel model)
    {
        model.Status = SalesOrderStatus.Draft;
        ModelState.Remove(nameof(model.Status));
        Normalize(model);
        ValidateOrder(model);
        if (!ModelState.IsValid)
        {
            LoadSelections();
            return View("Form", model);
        }

        var order = new SalesOrder();
        Apply(model, order);
        database.SalesOrders.Add(order);
        database.SaveChanges();
        return RedirectToAction(nameof(Details), new { id = order.Id });
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var order = database.SalesOrders.AsNoTracking().Include(item => item.Lines).FirstOrDefault(item => item.Id == id);
        if (order is null)
            return NotFound();
        if (IsFinal(order))
            return RedirectToAction(nameof(Details), new { id });

        LoadSelections(order);
        return View("Form", ToForm(order));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, SalesOrderFormViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        var order = database.SalesOrders.Include(item => item.Lines).FirstOrDefault(item => item.Id == id);
        if (order is null)
            return NotFound();
        if (IsFinal(order))
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        model.Status = order.Status;
        ModelState.Remove(nameof(model.Status));
        Normalize(model);
        ValidateOrder(model);
        if (!ModelState.IsValid)
        {
            LoadSelections(order);
            return View("Form", model);
        }

        database.SalesOrderLines.RemoveRange(order.Lines);
        Apply(model, order);
        database.SaveChanges();
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Confirm(int id)
    {
        var order = database.SalesOrders.Find(id);
        if (order is null)
            return NotFound();
        if (order.Status != SalesOrderStatus.Draft)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        order.Status = SalesOrderStatus.Confirmed;
        database.SaveChanges();
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Complete(int id)
    {
        var order = database.SalesOrders.Include(item => item.Lines).ThenInclude(line => line.Product).FirstOrDefault(item => item.Id == id);
        if (order is null)
            return NotFound();
        var stocks = database.WarehouseStocks.Where(stock => stock.WarehouseId == order.WarehouseId).ToDictionary(stock => stock.ProductId);
        if (order.Status != SalesOrderStatus.Confirmed)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        var insufficient = order.Lines.FirstOrDefault(line => !stocks.TryGetValue(line.ProductId, out var stock) || stock.Quantity < line.Quantity);
        if (insufficient is not null)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        using var transaction = database.Database.IsRelational() ? database.Database.BeginTransaction() : null;
        foreach (var line in order.Lines)
        {
            stocks[line.ProductId].Quantity -= line.Quantity;
            line.Product.Stock -= line.Quantity;
            database.StockMovements.Add(new() { Type = StockMovementType.Exit, ProductId = line.ProductId, SourceWarehouseId = order.WarehouseId, Quantity = line.Quantity, Reference = order.Number });
        }
        order.Status = SalesOrderStatus.Completed;
        database.SaveChanges();
        transaction?.Commit();
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancel(int id)
    {
        var order = database.SalesOrders.Find(id);
        if (order is null)
            return NotFound();
        if (order.Status is SalesOrderStatus.Completed or SalesOrderStatus.Cancelled)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        order.Status = SalesOrderStatus.Cancelled;
        database.SaveChanges();
        return RedirectToAction(nameof(Details), new { id });
    }

    private void ValidateOrder(SalesOrderFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Number) || model.Number.Length > 30)
            ModelState.AddModelError(nameof(model.Number), "El número no es válido.");
        if (database.SalesOrders.Any(order => order.Id != model.Id && order.Number == model.Number))
            ModelState.AddModelError(nameof(model.Number), "Ya existe un pedido con este número.");

        var customer = database.Customers.AsNoTracking().FirstOrDefault(item => item.Id == model.CustomerId);
        if (customer is null || !customer.IsActive)
            ModelState.AddModelError(nameof(model.CustomerId), "Selecciona un cliente activo.");

        if (model.Lines.Count == 0)
            ModelState.AddModelError(nameof(model.Lines), "El pedido debe tener al menos una línea.");
        if (model.Lines.Where(line => line.ProductId > 0).GroupBy(line => line.ProductId).Any(group => group.Count() > 1))
            ModelState.AddModelError(nameof(model.Lines), "No se puede repetir un artículo.");
        if (model.Lines.Any(line => line.Quantity is <= 0 or > 999999999.999m))
            ModelState.AddModelError(nameof(model.Lines), "La cantidad de las líneas no es válida.");
        if (model.Lines.Any(line => line.UnitPrice is < 0 or > 999999999.99m))
            ModelState.AddModelError(nameof(model.Lines), "El precio de las líneas no es válido.");
        if (model.Lines.Any(line => line.DiscountPercentage is < 0 or > 100))
            ModelState.AddModelError(nameof(model.Lines), "El descuento de las líneas no es válido.");

        var productIds = model.Lines.Select(line => line.ProductId).Distinct().ToArray();
        var activeProducts = database.Products.AsNoTracking().Where(product => productIds.Contains(product.Id) && product.IsActive).Select(product => product.Id).ToHashSet();
        if (!database.Warehouses.Any(item => item.Id == model.WarehouseId && item.IsActive))
            ModelState.AddModelError(nameof(model.WarehouseId), "Selecciona un almacén activo.");
        if (productIds.Any(id => !activeProducts.Contains(id)))
            ModelState.AddModelError(nameof(model.Lines), "Todas las líneas deben usar artículos activos.");
    }

    private static void Apply(SalesOrderFormViewModel model, SalesOrder order)
    {
        order.Number = model.Number;
        order.OrderDate = model.OrderDate;
        order.CustomerId = model.CustomerId;
        order.WarehouseId = model.WarehouseId;
        order.Lines = model.Lines.Select(line => new SalesOrderLine
        {
            ProductId = line.ProductId,
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice,
            DiscountPercentage = line.DiscountPercentage
        }).ToList();
    }

    private void LoadSelections(SalesOrder? order = null)
    {
        var customerId = order?.CustomerId;
        var customerQuery = database.Customers.AsNoTracking().Where(item => item.IsActive || item.Id == customerId);
        ViewBag.Customers = new SelectList(customerQuery.OrderBy(item => item.Name), "Id", "Name", customerId);

        var productIds = order?.Lines.Select(line => line.ProductId).ToArray() ?? [];
        var products = database.Products.AsNoTracking().Where(item => item.IsActive || productIds.Contains(item.Id)).OrderBy(item => item.Name).ToArray();
        ViewBag.Products = products;
        var warehouseId = order?.WarehouseId;
        ViewBag.Warehouses = new SelectList(database.Warehouses.AsNoTracking().Where(item => item.IsActive || item.Id == warehouseId).OrderBy(item => item.Name), "Id", "Name", warehouseId);
    }

    private static SalesOrderFormViewModel ToForm(SalesOrder order) => new()
    {
        Id = order.Id,
        Number = order.Number,
        OrderDate = order.OrderDate,
        CustomerId = order.CustomerId,
        Status = order.Status,
        WarehouseId = order.WarehouseId,
        Lines = order.Lines.Select(line => new SalesOrderLineInput
        {
            Id = line.Id,
            ProductId = line.ProductId,
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice,
            DiscountPercentage = line.DiscountPercentage
        }).ToList()
    };

    private static void Normalize(SalesOrderFormViewModel model)
    {
        model.Number = model.Number?.Trim().ToUpperInvariant() ?? string.Empty;
        model.Lines ??= [];
    }

    private static bool IsFinal(SalesOrder order) => order.Status is SalesOrderStatus.Completed or SalesOrderStatus.Cancelled;
}