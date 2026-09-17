using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniErp.Controllers;
using MiniErp.Data;
using MiniErp.Models;
using MiniErp.ViewModels;

namespace MiniErp.Tests;

public class SalesOrdersControllerTests
{
    [Fact]
    public void Create_RejectsDuplicateNumber()
    {
        using var database = CreateDatabase();
        Seed(database);
        database.SalesOrders.Add(new SalesOrder { Number = "PV-1", CustomerId = 1 });
        database.SaveChanges();
        var controller = new SalesOrdersController(database);
        var model = ValidModel();
        model.Number = "pv-1";

        var result = controller.Create(model);

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public void Create_RejectsRepeatedProducts()
    {
        using var database = CreateDatabase();
        Seed(database);
        var controller = new SalesOrdersController(database);
        var model = ValidModel();
        model.Lines.Add(new SalesOrderLineInput { ProductId = 1, Quantity = 2, UnitPrice = 10 });

        var validation = model.Validate(new System.ComponentModel.DataAnnotations.ValidationContext(model));

        Assert.Contains(validation, item => item.MemberNames.Contains(nameof(model.Lines)));
    }

    [Fact]
    public void Complete_DecreasesStockOnce()
    {
        using var database = CreateDatabase();
        Seed(database);
        var order = new SalesOrder
        {
            Number = "PV-1",
            CustomerId = 1,
            WarehouseId = 1,
            Status = SalesOrderStatus.Confirmed,
            Lines = [new SalesOrderLine { ProductId = 1, Quantity = 2, UnitPrice = 10 }]
        };
        database.SalesOrders.Add(order);
        database.SaveChanges();
        var controller = new SalesOrdersController(database);

        controller.Complete(order.Id);
        controller.Complete(order.Id);

        Assert.Equal(8m, database.Products.Single().Stock);
        Assert.Equal(SalesOrderStatus.Completed, order.Status);
    }

    [Fact]
    public void Complete_RejectsInsufficientStock()
    {
        using var database = CreateDatabase();
        Seed(database);
        var order = new SalesOrder
        {
            Number = "PV-1",
            CustomerId = 1,
            Status = SalesOrderStatus.Confirmed,
            Lines = [new SalesOrderLine { ProductId = 1, Quantity = 11, UnitPrice = 10 }]
        };
        database.SalesOrders.Add(order);
        database.SaveChanges();
        var controller = new SalesOrdersController(database);

        controller.Complete(order.Id);

        Assert.Equal(10m, database.Products.Single().Stock);
        Assert.Equal(SalesOrderStatus.Confirmed, order.Status);
    }

    [Theory]
    [InlineData(SalesOrderStatus.Completed)]
    [InlineData(SalesOrderStatus.Cancelled)]
    public void Edit_BlocksFinalOrders(SalesOrderStatus status)
    {
        using var database = CreateDatabase();
        Seed(database);
        var order = new SalesOrder { Number = "PV-1", CustomerId = 1, WarehouseId = 1, Status = status };
        database.SalesOrders.Add(order);
        database.SaveChanges();
        var controller = new SalesOrdersController(database);

        var result = controller.Edit(order.Id, ValidModel(order.Id));

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("PV-1", order.Number);
    }

    [Fact]
    public void LineTotal_AppliesDiscount()
    {
        var line = new SalesOrderLine { Quantity = 3, UnitPrice = 20, DiscountPercentage = 10 };
        Assert.Equal(54m, line.LineTotal);
    }

    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(-1, 10, 0)]
    [InlineData(1, -1, 0)]
    [InlineData(1, 10, -1)]
    [InlineData(1, 10, 101)]
    public void Create_RejectsInvalidLineValuesWithoutPersisting(decimal quantity, decimal unitPrice, decimal discount)
    {
        using var database = CreateDatabase();
        Seed(database);
        var controller = new SalesOrdersController(database);
        var model = ValidModel();
        model.Lines[0].Quantity = quantity;
        model.Lines[0].UnitPrice = unitPrice;
        model.Lines[0].DiscountPercentage = discount;

        var result = controller.Create(model);

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Empty(database.SalesOrders);
    }

    [Fact]
    public void Create_RejectsEmptyLinesWithoutPersisting()
    {
        using var database = CreateDatabase();
        Seed(database);
        var controller = new SalesOrdersController(database);
        var model = ValidModel();
        model.Lines = [];

        var result = controller.Create(model);

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Empty(database.SalesOrders);
    }

    private static SalesOrderFormViewModel ValidModel(int id = 0) => new()
    {
        Id = id,
        Number = "PV-2",
        CustomerId = 1,
        WarehouseId = 1,
        Status = SalesOrderStatus.Draft,
        Lines = [new SalesOrderLineInput { ProductId = 1, Quantity = 1, UnitPrice = 10 }]
    };

    private static AppDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new AppDbContext(options);
    }

    private static void Seed(AppDbContext database)
    {
        database.Customers.Add(new Customer { Id = 1, Name = "Cliente", TaxId = "12345678Z", IsActive = true });
        database.Warehouses.Add(new Warehouse { Id = 1, Code = "MAIN", Name = "Principal" });
        database.Products.Add(new Product { Id = 1, Sku = "A-1", Name = "Artículo", UnitPrice = 10, Stock = 10, IsActive = true });
        database.WarehouseStocks.Add(new WarehouseStock { WarehouseId = 1, ProductId = 1, Quantity = 10 });
        database.SaveChanges();
    }
}