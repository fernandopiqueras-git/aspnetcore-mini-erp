using Microsoft.EntityFrameworkCore;
using MiniErp.Controllers;
using MiniErp.Data;
using MiniErp.Models;
using MiniErp.ViewModels;

namespace MiniErp.Tests;

public class StockMovementSecurityTests
{
    [Fact]
    public void ManualReceipt_ClearsValidationFromForgedExitType()
    {
        using var database = CreateDatabase();
        Seed(database);
        var controller = new StockMovementsController(database);
        controller.ModelState.AddModelError(nameof(StockMovementViewModel.SourceWarehouseId), "Origen obligatorio.");
        var model = new StockMovementViewModel
        {
            Type = StockMovementType.Exit,
            ProductId = 1,
            DestinationWarehouseId = 1,
            Quantity = 2,
            Reference = "MANUAL"
        };

        controller.Create(model);

        Assert.True(controller.ModelState.IsValid);
        var movement = Assert.Single(database.StockMovements);
        Assert.Equal(StockMovementType.Entry, movement.Type);
        Assert.Null(movement.SourceWarehouseId);
        Assert.Equal(1, movement.DestinationWarehouseId);
        Assert.Equal(12m, database.Products.Single().Stock);
        Assert.Equal(12m, database.WarehouseStocks.Single().Quantity);
    }

    [Fact]
    public void ManualReceipt_RejectsInactiveProductWithoutChangingStock()
    {
        using var database = CreateDatabase();
        Seed(database);
        database.Products.Single().IsActive = false;
        database.SaveChanges();
        var controller = new StockMovementsController(database);

        controller.Create(new StockMovementViewModel
        {
            ProductId = 1,
            DestinationWarehouseId = 1,
            Quantity = 2,
            Reference = "MANUAL"
        });

        Assert.False(controller.ModelState.IsValid);
        Assert.Empty(database.StockMovements);
        Assert.Equal(10m, database.Products.Single().Stock);
        Assert.Equal(10m, database.WarehouseStocks.Single().Quantity);
    }

    [Fact]
    public void ManualReceipt_RejectsUnknownWarehouseWithoutCreatingBalance()
    {
        using var database = CreateDatabase();
        Seed(database);
        var controller = new StockMovementsController(database);

        controller.Create(new StockMovementViewModel
        {
            ProductId = 1,
            DestinationWarehouseId = 999,
            Quantity = 2,
            Reference = "MANUAL"
        });

        Assert.False(controller.ModelState.IsValid);
        Assert.Empty(database.StockMovements);
        Assert.Single(database.WarehouseStocks);
    }

    private static AppDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static void Seed(AppDbContext database)
    {
        database.AddRange(
            new Warehouse { Id = 1, Code = "MAIN", Name = "Principal", IsActive = true },
            new Product { Id = 1, Sku = "A", Name = "Artículo", Stock = 10, IsActive = true },
            new WarehouseStock { WarehouseId = 1, ProductId = 1, Quantity = 10 });
        database.SaveChanges();
    }
}
