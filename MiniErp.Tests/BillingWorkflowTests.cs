using System.Globalization;
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
    public void LocalizedPaymentAmount_IsParsedCorrectly()
    {
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("es-ES");
            using var database = CreateDatabase();
            var invoice = AddInvoice(database, 1234.56m);

            var result = new InvoicesController(database).Pay(invoice.Id, "1.234,56", "Transferencia");

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(InvoiceStatus.Paid, invoice.Status);
            Assert.Equal(0m, invoice.Outstanding);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
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
    public void UnknownPaymentMethod_IsRejected()
    {
        using var database = CreateDatabase();
        var invoice = AddInvoice(database, 121);

        var result = new InvoicesController(database).Pay(invoice.Id, 20, "Criptomonedas");

        Assert.IsType<BadRequestResult>(result);
        Assert.Empty(invoice.Payments);
        Assert.Equal(InvoiceStatus.Issued, invoice.Status);
    }

    [Theory]
    [InlineData(InvoiceStatus.Draft)]
    [InlineData(InvoiceStatus.Issued)]
    public void InvoiceWithoutPayments_CanBeCancelled(InvoiceStatus initialStatus)
    {
        using var database = CreateDatabase();
        var invoice = AddInvoice(database, 121);
        invoice.Status = initialStatus;
        database.SaveChanges();

        var result = new InvoicesController(database).Cancel(invoice.Id);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(InvoiceStatus.Cancelled, invoice.Status);
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

    [Theory]
    [InlineData(InvoiceStatus.Draft)]
    [InlineData(InvoiceStatus.Paid)]
    [InlineData(InvoiceStatus.Cancelled)]
    public void Payment_IsRejectedOutsideIssuedState(InvoiceStatus status)
    {
        using var database = CreateDatabase();
        var invoice = AddInvoice(database, 121);
        invoice.Status = status;
        database.SaveChanges();

        var result = new InvoicesController(database).Pay(invoice.Id, 20, "Transferencia");

        Assert.IsType<BadRequestResult>(result);
        Assert.Empty(invoice.Payments);
        Assert.Equal(status, invoice.Status);
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

    [Fact]
    public void RepeatedSaleInvoiceCreationReturnsTheExistingInvoice()
    {
        using var database = CreateDatabase();
        SeedCompletedSales(database);
        var controller = new InvoicesController(database);

        controller.FromSale(1, "V", 21);
        var existing = Assert.Single(database.Invoices);
        var result = controller.FromSale(1, "V", 21);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(InvoicesController.Details), redirect.ActionName);
        Assert.Equal(existing.Id, redirect.RouteValues!["id"]);
        Assert.Single(database.Invoices);
    }

    [Fact]
    public void Index_FiltersInvoicesAndCalculatesAccumulatedAmounts()
    {
        using var database = CreateDatabase();
        database.AddRange(
            new Customer { Id = 1, Name = "Cliente uno", TaxId = "12345678Z" },
            new Customer { Id = 2, Name = "Cliente dos", TaxId = "87654321X" },
            new Warehouse { Id = 1, Code = "MAIN", Name = "Principal" },
            new SalesOrder { Id = 1, Number = "PV-1", CustomerId = 1, WarehouseId = 1 },
            new SalesOrder { Id = 2, Number = "PV-2", CustomerId = 2, WarehouseId = 1 });
        database.SaveChanges();

        database.AddRange(
            new Invoice
            {
                Type = InvoiceType.Sale,
                Series = "V",
                Number = "2026-00001",
                IssueDate = new DateTime(2026, 5, 10),
                Status = InvoiceStatus.Issued,
                SalesOrderId = 1,
                TaxBase = 100,
                TaxAmount = 21,
                Total = 121,
                Payments = [new Payment { Amount = 20, Method = "Transferencia" }]
            },
            new Invoice
            {
                Type = InvoiceType.Sale,
                Series = "V",
                Number = "2026-00002",
                IssueDate = new DateTime(2026, 6, 10),
                Status = InvoiceStatus.Issued,
                SalesOrderId = 2,
                TaxBase = 200,
                TaxAmount = 42,
                Total = 242
            });
        database.SaveChanges();

        var result = new InvoicesController(database).Index(
            type: InvoiceType.Sale,
            year: 2026,
            month: 5,
            customerId: 1);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MiniErp.ViewModels.InvoiceIndexViewModel>(view.Model);
        Assert.Single(model.Invoices);
        Assert.Equal(100m, model.AccumulatedTaxBase);
        Assert.Equal(21m, model.AccumulatedTax);
        Assert.Equal(121m, model.AccumulatedTotal);
        Assert.Equal(20m, model.AccumulatedPaid);
        Assert.Equal(101m, model.AccumulatedOutstanding);
    }

    [Fact]
    public void Index_DoesNotAccumulateCancelledInvoices()
    {
        using var database = CreateDatabase();
        var active = AddInvoice(database, 121);
        active.TaxBase = 100;
        active.TaxAmount = 21;
        var cancelled = new Invoice
        {
            Series = "V",
            Number = "2026-00002",
            Status = InvoiceStatus.Cancelled,
            TaxBase = 500,
            TaxAmount = 105,
            Total = 605
        };
        database.Add(cancelled);
        database.SaveChanges();

        var result = new InvoicesController(database).Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MiniErp.ViewModels.InvoiceIndexViewModel>(view.Model);
        Assert.Equal(2, model.Invoices.Count);
        Assert.Equal(100m, model.AccumulatedTaxBase);
        Assert.Equal(21m, model.AccumulatedTax);
        Assert.Equal(121m, model.AccumulatedTotal);
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
