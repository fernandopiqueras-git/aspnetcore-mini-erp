using Microsoft.EntityFrameworkCore;
using MiniErp.Controllers;
using MiniErp.Data;
using MiniErp.Models;
using MiniErp.ViewModels;

namespace MiniErp.Tests;

public class OrderWorkflowSecurityTests
{
    [Fact]
    public void SalesOrderCreate_IgnoresForgedCompletedStatus()
    {
        using var database = CreateDatabase();
        Seed(database);
        var model = SalesModel();
        model.Status = SalesOrderStatus.Completed;

        new SalesOrdersController(database).Create(model);

        Assert.Equal(SalesOrderStatus.Draft, database.SalesOrders.Single().Status);
    }

    [Fact]
    public void SalesOrderEdit_PreservesConfirmedStatus()
    {
        using var database = CreateDatabase();
        Seed(database);
        var order = new SalesOrder
        {
            Number = "PV-1",
            CustomerId = 1,
            WarehouseId = 1,
            Status = SalesOrderStatus.Confirmed,
            Lines = [new SalesOrderLine { ProductId = 1, Quantity = 1, UnitPrice = 10 }]
        };
        database.Add(order);
        database.SaveChanges();
        var model = SalesModel(order.Id);
        model.Status = SalesOrderStatus.Draft;

        new SalesOrdersController(database).Edit(order.Id, model);

        Assert.Equal(SalesOrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void PurchaseOrderCreate_IgnoresForgedReceivedStatus()
    {
        using var database = CreateDatabase();
        Seed(database);
        var model = PurchaseModel();
        model.Status = PurchaseOrderStatus.Received;

        new PurchaseOrdersController(database).Create(model);

        Assert.Equal(PurchaseOrderStatus.Draft, database.PurchaseOrders.Single().Status);
    }

    [Fact]
    public void PurchaseOrderEdit_PreservesConfirmedStatus()
    {
        using var database = CreateDatabase();
        Seed(database);
        var order = new PurchaseOrder
        {
            Number = "PC-1",
            SupplierId = 1,
            WarehouseId = 1,
            Status = PurchaseOrderStatus.Confirmed,
            Lines = [new PurchaseOrderLine { ProductId = 1, Quantity = 1, UnitPrice = 10 }]
        };
        database.Add(order);
        database.SaveChanges();
        var model = PurchaseModel(order.Id);
        model.Status = PurchaseOrderStatus.Draft;

        new PurchaseOrdersController(database).Edit(order.Id, model);

        Assert.Equal(PurchaseOrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void CompletingSale_CreatesOneIssueAndDecreasesWarehouseStockOnce()
    {
        using var database = CreateDatabase();
        Seed(database);
        var order = new SalesOrder
        {
            Number = "PV-1",
            CustomerId = 1,
            WarehouseId = 1,
            Status = SalesOrderStatus.Confirmed,
            Lines = [new SalesOrderLine { ProductId = 1, Quantity = 3, UnitPrice = 10 }]
        };
        database.Add(order);
        database.SaveChanges();
        var controller = new SalesOrdersController(database);

        controller.Complete(order.Id);
        controller.Complete(order.Id);

        var movement = Assert.Single(database.StockMovements);
        Assert.Equal(StockMovementType.Exit, movement.Type);
        Assert.Equal("PV-1", movement.Reference);
        Assert.Equal(7m, database.WarehouseStocks.Single().Quantity);
        Assert.Equal(7m, database.Products.Single().Stock);
    }

    [Fact]
    public void ReceivingPurchase_CreatesOneReceiptAndIncreasesWarehouseStockOnce()
    {
        using var database = CreateDatabase();
        Seed(database);
        var order = new PurchaseOrder
        {
            Number = "PC-1",
            SupplierId = 1,
            WarehouseId = 1,
            Status = PurchaseOrderStatus.Confirmed,
            Lines = [new PurchaseOrderLine { ProductId = 1, Quantity = 4, UnitPrice = 6 }]
        };
        database.Add(order);
        database.SaveChanges();
        var controller = new PurchaseOrdersController(database);

        controller.Receive(order.Id);
        controller.Receive(order.Id);

        var movement = Assert.Single(database.StockMovements);
        Assert.Equal(StockMovementType.Entry, movement.Type);
        Assert.Equal("PC-1", movement.Reference);
        Assert.Equal(14m, database.WarehouseStocks.Single().Quantity);
        Assert.Equal(14m, database.Products.Single().Stock);
    }

    private static SalesOrderFormViewModel SalesModel(int id = 0) => new()
    {
        Id = id,
        Number = "PV-2",
        CustomerId = 1,
        WarehouseId = 1,
        Lines = [new SalesOrderLineInput { ProductId = 1, Quantity = 1, UnitPrice = 10 }]
    };

    private static PurchaseOrderFormViewModel PurchaseModel(int id = 0) => new()
    {
        Id = id,
        Number = "PC-2",
        SupplierId = 1,
        WarehouseId = 1,
        Lines = [new PurchaseOrderLineInput { ProductId = 1, Quantity = 1, UnitPrice = 10 }]
    };

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
            new Customer { Id = 1, Name = "Cliente", TaxId = "12345678Z", IsActive = true },
            new Supplier { Id = 1, Name = "Proveedor", TaxId = "B45000007", IsActive = true },
            new Warehouse { Id = 1, Code = "MAIN", Name = "Principal", IsActive = true },
            new Product { Id = 1, Sku = "A", Name = "Artículo", UnitPrice = 10, Stock = 10, IsActive = true },
            new WarehouseStock { WarehouseId = 1, ProductId = 1, Quantity = 10 });
        database.SaveChanges();
    }
}
