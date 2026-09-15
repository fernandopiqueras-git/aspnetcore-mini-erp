using Microsoft.EntityFrameworkCore;
using MiniErp.Data;
using MiniErp.Models;

namespace MiniErp.Tests;

public class AppDbContextTests
{
    [Fact]
    public void Model_HasTheExpectedRelationshipsAndDeleteBehaviors()
    {
        using var database = CreateDatabase();
        var orderCustomer = database.Model.FindEntityType(typeof(SalesOrder))!.GetForeignKeys().Single();
        var lineRelationships = database.Model.FindEntityType(typeof(SalesOrderLine))!.GetForeignKeys().ToArray();

        Assert.Equal(DeleteBehavior.Restrict, orderCustomer.DeleteBehavior);
        Assert.Equal(DeleteBehavior.Cascade, lineRelationships.Single(key => key.PrincipalEntityType.ClrType == typeof(SalesOrder)).DeleteBehavior);
        Assert.Equal(DeleteBehavior.Restrict, lineRelationships.Single(key => key.PrincipalEntityType.ClrType == typeof(Product)).DeleteBehavior);
    }

    [Fact]
    public void DemoData_SeedsACompleteSalesOrderOnlyOnce()
    {
        using var database = CreateDatabase();

        DemoData.Seed(database);
        DemoData.Seed(database);

        Assert.Single(database.Customers);
        Assert.Equal(2, database.Products.Count());
        var order = database.SalesOrders.Include(item => item.Lines).Single();
        Assert.Equal(2, order.Lines.Count);
        Assert.True(order.Total > 0);
    }

    [Fact]
    public void Model_HasUniqueBusinessIdentifiers()
    {
        using var database = CreateDatabase();

        Assert.True(database.Model.FindEntityType(typeof(Customer))!.GetIndexes().Single(index => index.Properties.Single().Name == nameof(Customer.TaxId)).IsUnique);
        Assert.True(database.Model.FindEntityType(typeof(Product))!.GetIndexes().Single(index => index.Properties.Single().Name == nameof(Product.Sku)).IsUnique);
        Assert.True(database.Model.FindEntityType(typeof(SalesOrder))!.GetIndexes().Single(index => index.Properties.Single().Name == nameof(SalesOrder.Number)).IsUnique);
    }

    private static AppDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
