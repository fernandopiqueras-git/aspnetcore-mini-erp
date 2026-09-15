using MiniErp.Models;
using System.ComponentModel.DataAnnotations;

namespace MiniErp.Tests;

public class ModelValidationTests
{
    [Fact]
    public void Customer_RequiresNameAndTaxId()
    {
        var results = Validate(new Customer());

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(Customer.Name)));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(Customer.TaxId)));
    }

    [Fact]
    public void Product_RejectsNegativePriceAndStock()
    {
        var results = Validate(new Product { Sku = "ART-001", Name = "Artículo", UnitPrice = -1m, Stock = -1m });

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(Product.UnitPrice)));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(Product.Stock)));
    }

    [Fact]
    public void SalesOrderLine_CalculatesDiscountedTotal()
    {
        var line = new SalesOrderLine { Quantity = 3m, UnitPrice = 20m, DiscountPercentage = 10m };

        Assert.Equal(54m, line.LineTotal);
    }

    [Fact]
    public void SalesOrder_TotalIsTheSumOfItsLines()
    {
        var order = new SalesOrder
        {
            Lines =
            [
                new SalesOrderLine { Quantity = 2m, UnitPrice = 10m },
                new SalesOrderLine { Quantity = 1m, UnitPrice = 5m }
            ]
        };

        Assert.Equal(25m, order.Total);
    }

    private static IReadOnlyCollection<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, true);
        return results;
    }
}
