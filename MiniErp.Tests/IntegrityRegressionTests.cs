using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniErp.Controllers;
using MiniErp.Data;
using MiniErp.Models;
using MiniErp.ViewModels;

namespace MiniErp.Tests;

public class IntegrityRegressionTests
{
    [Fact]
    public void ProductEdit_DoesNotChangeStockOutsideInventory()
    {
        using var database = CreateDatabase();
        database.Products.Add(new Product { Id = 1, Sku = "A", Name = "Artículo", Stock = 8, UnitPrice = 2 });
        database.SaveChanges();

        new ProductsController(database).Edit(1, new Product { Id = 1, Sku = "A", Name = "Artículo editado", Stock = 999, UnitPrice = 3 });

        Assert.Equal(8m, database.Products.Single().Stock);
    }

    [Fact]
    public void ManualReceipt_IgnoresManipulatedMovementType()
    {
        using var database = CreateDatabase();
        database.AddRange(
            new Warehouse { Id = 1, Code = "MAIN", Name = "Principal" },
            new Product { Id = 1, Sku = "A", Name = "Artículo", IsActive = true });
        database.SaveChanges();

        new StockMovementsController(database).ManualEntry(new StockMovementViewModel
        {
            Type = StockMovementType.Adjustment,
            ProductId = 1,
            DestinationWarehouseId = 1,
            Quantity = 4,
            Reference = "MANUAL"
        });

        Assert.Equal(StockMovementType.Entry, database.StockMovements.Single().Type);
        Assert.Equal(4m, database.Products.Single().Stock);
    }

    [Fact]
    public void InvoiceModel_PreservesUniqueOrderLinks()
    {
        using var database = CreateDatabase();
        var invoice = database.Model.FindEntityType(typeof(Invoice))!;

        Assert.True(invoice.GetIndexes().Single(index => index.Properties.Count == 1 && index.Properties[0].Name == nameof(Invoice.SalesOrderId)).IsUnique);
        Assert.True(invoice.GetIndexes().Single(index => index.Properties.Count == 1 && index.Properties[0].Name == nameof(Invoice.PurchaseOrderId)).IsUnique);
    }

    [Fact]
    public void InvoiceCreation_RejectsInvalidTaxRate()
    {
        using var database = CreateDatabase();
        database.AddRange(
            new Customer { Id = 1, Name = "Cliente", TaxId = "B45000007" },
            new Warehouse { Id = 1, Code = "MAIN", Name = "Principal" },
            new SalesOrder { Id = 1, Number = "PV-1", CustomerId = 1, WarehouseId = 1, Status = SalesOrderStatus.Completed });
        database.SaveChanges();

        var result = new InvoicesController(database).FromSale(1, "V", 101);

        Assert.IsType<BadRequestResult>(result);
        Assert.Empty(database.Invoices);
    }

    private static AppDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
