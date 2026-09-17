using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using MiniErp.Controllers;
using MiniErp.Data;
using MiniErp.Models;

namespace MiniErp.Tests;

public class MasterDataControllerTests
{
    [Fact]
    public void CustomerCreate_RejectsDuplicateTaxId()
    {
        using var database = CreateDatabase();
        database.Customers.Add(new Customer { Name = "Existente", TaxId = "12345678Z" });
        database.SaveChanges();

        var result = new CustomersController(database).Create(new Customer { Name = "Nuevo", TaxId = "12345678Z" });

        Assert.IsType<ViewResult>(result);
        Assert.Single(database.Customers);
    }

    [Fact]
    public void CustomerCreate_RejectsFormattedDuplicateTaxId()
    {
        using var database = CreateDatabase();
        database.Customers.Add(new Customer { Name = "Existente", TaxId = "12345678Z" });
        database.SaveChanges();
        var controller = new CustomersController(database);

        var result = controller.Create(new Customer { Name = "Nuevo", TaxId = "12.345.678-Z" });

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Single(database.Customers);
    }

    [Fact]
    public void ProductCreate_NormalizesAndPersistsAValidProduct()
    {
        using var database = CreateDatabase();

        var result = new ProductsController(database).Create(new Product { Sku = " art-003 ", Name = " Artículo ", UnitPrice = 10m, Stock = 2m });

        Assert.IsType<RedirectToActionResult>(result);
        var product = Assert.Single(database.Products);
        Assert.Equal("ART-003", product.Sku);
        Assert.Equal("Artículo", product.Name);
    }

    [Fact]
    public void CustomerDelete_IsBlockedWhenItHasOrders()
    {
        using var database = CreateDatabase();
        var customer = new Customer { Name = "Cliente", TaxId = "12345678Z" };
        database.SalesOrders.Add(new SalesOrder { Number = "PV-1", Customer = customer });
        database.SaveChanges();
        var controller = new CustomersController(database)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider())
        };

        var result = controller.DeleteConfirmed(customer.Id);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Single(database.Customers);
    }

    [Fact]
    public void ProductDelete_IsBlockedWhenItHasOrderLines()
    {
        using var database = CreateDatabase();
        var customer = new Customer { Name = "Cliente", TaxId = "12345678Z" };
        var product = new Product { Sku = "ART-1", Name = "Artículo" };
        database.SalesOrders.Add(new SalesOrder
        {
            Number = "PV-1",
            Customer = customer,
            Lines = [new SalesOrderLine { Product = product, Quantity = 1m }]
        });
        database.SaveChanges();
        var controller = new ProductsController(database)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider())
        };

        var result = controller.DeleteConfirmed(product.Id);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Single(database.Products);
    }

    [Fact]
    public void WriteActions_RequireAntiforgeryTokens()
    {
        var actions = new[]
        {
            typeof(CustomersController).GetMethod(nameof(CustomersController.Create), [typeof(Customer)])!,
            typeof(CustomersController).GetMethod(nameof(CustomersController.Edit), [typeof(int), typeof(Customer)])!,
            typeof(CustomersController).GetMethod(nameof(CustomersController.DeleteConfirmed))!,
            typeof(ProductsController).GetMethod(nameof(ProductsController.Create), [typeof(Product)])!,
            typeof(ProductsController).GetMethod(nameof(ProductsController.Edit), [typeof(int), typeof(Product)])!,
            typeof(ProductsController).GetMethod(nameof(ProductsController.DeleteConfirmed))!
        };

        Assert.All(actions, action => Assert.NotNull(action.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), false).SingleOrDefault()));
    }

    private static AppDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new AppDbContext(options);
    }

    private sealed class TestTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
