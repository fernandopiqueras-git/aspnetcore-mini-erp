using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniErp.Controllers;
using MiniErp.Data;
using MiniErp.Models;

namespace MiniErp.Tests;

public class BillingWorkflowTests
{
    [Fact]
    public void PartialPayment_UpdatesOutstandingAndKeepsInvoiceIssued()
    {
        using var database = CreateDatabase();
        var invoice = AddInvoice(database, 121);
        var controller = new InvoicesController(database);

        controller.Pay(invoice.Id, 40, "Transferencia");

        Assert.Equal(81m, invoice.Outstanding);
        Assert.Equal(InvoiceStatus.Issued, invoice.Status);
        Assert.Single(invoice.Payments);
    }

    [Fact]
    public void ExactPayment_MarksInvoiceAsPaid()
    {
        using var database = CreateDatabase();
        var invoice = AddInvoice(database, 121);

        new InvoicesController(database).Pay(invoice.Id, 121, "Transferencia");

        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.Equal(0m, invoice.Outstanding);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(122)]
    public void InvalidPayment_IsRejected(decimal amount)
    {
        using var database = CreateDatabase();
        var invoice = AddInvoice(database, 121);

        var result = new InvoicesController(database).Pay(invoice.Id, amount, "Transferencia");

        Assert.IsType<BadRequestResult>(result);
        Assert.Empty(invoice.Payments);
        Assert.Equal(InvoiceStatus.Issued, invoice.Status);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyPaymentMethod_IsRejected(string method)
    {
        using var database = CreateDatabase();
        var invoice = AddInvoice(database, 121);

        var result = new InvoicesController(database).Pay(invoice.Id, 20, method);

        Assert.IsType<BadRequestResult>(result);
        Assert.Empty(invoice.Payments);
    }

    [Fact]
    public void InvoiceWithPayments_CannotBeCancelled()
    {
        using var database = CreateDatabase();
        var invoice = AddInvoice(database, 121);
        var controller = new InvoicesController(database);
        controller.Pay(invoice.Id, 20, "Transferencia");

        controller.Cancel(invoice.Id);

        Assert.Equal(InvoiceStatus.Issued, invoice.Status);
    }

    [Fact]
    public void SaleInvoices_UseSequentialNumbersPerYearAndSeries()
    {
        using var database = CreateDatabase();
        SeedCompletedSales(database);
        var controller = new InvoicesController(database);

        controller.FromSale(1, "V", 21);
        controller.FromSale(2, "V", 21);

        var invoices = database.Invoices.OrderBy(invoice => invoice.Id).ToArray();
        Assert.Equal($"{DateTime.Today.Year}-00001", invoices[0].Number);
        Assert.Equal($"{DateTime.Today.Year}-00002", invoices[1].Number);
    }

    private static Invoice AddInvoice(AppDbContext database, decimal total)
    {
        var invoice = new Invoice
        {
            Series = "V",
            Number = $"{DateTime.Today.Year}-00001",
            Status = InvoiceStatus.Issued,
            TaxBase = total,
            Total = total
        };
        database.Add(invoice);
        database.SaveChanges();
        return invoice;
    }

    private static void SeedCompletedSales(AppDbContext database)
    {
        database.AddRange(
            new Customer { Id = 1, Name = "Cliente", TaxId = "12345678Z" },
            new Warehouse { Id = 1, Code = "MAIN", Name = "Principal" },
            new SalesOrder { Id = 1, Number = "PV-1", CustomerId = 1, WarehouseId = 1, Status = SalesOrderStatus.Completed },
            new SalesOrder { Id = 2, Number = "PV-2", CustomerId = 1, WarehouseId = 1, Status = SalesOrderStatus.Completed });
        database.SaveChanges();
    }

    private static AppDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
