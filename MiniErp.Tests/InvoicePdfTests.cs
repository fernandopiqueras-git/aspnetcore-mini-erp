using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MiniErp.Controllers;
using MiniErp.Data;
using MiniErp.Models;
using MiniErp.Options;
using MiniErp.Services;
using QuestPDF.Infrastructure;

namespace MiniErp.Tests;

public class InvoicePdfTests
{
    [Fact]
    public void Pdf_GeneratesAnA4InvoiceForASale()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        using var database = CreateDatabase();
        database.AddRange(
            new Customer { Id = 1, Name = "Cliente PDF", TaxId = "12345678Z", Address = "Calle Mayor 1" },
            new Warehouse { Id = 1, Code = "MAIN", Name = "Principal" },
            new Product { Id = 1, Sku = "ART-001", Name = "Artículo", UnitPrice = 100 },
            new SalesOrder
            {
                Id = 1,
                Number = "PV-2026-0001",
                CustomerId = 1,
                WarehouseId = 1,
                Status = SalesOrderStatus.Completed,
                Lines =
                [
                    new SalesOrderLine
                    {
                        ProductId = 1,
                        Quantity = 2,
                        UnitPrice = 100,
                        DiscountPercentage = 10
                    }
                ]
            });
        database.SaveChanges();
        var invoice = new Invoice
        {
            Type = InvoiceType.Sale,
            Series = "V",
            Number = "2026-00001",
            Status = InvoiceStatus.Issued,
            SalesOrderId = 1,
            TaxBase = 180,
            TaxRate = 21,
            TaxAmount = 37.80m,
            Total = 217.80m
        };
        database.Add(invoice);
        database.SaveChanges();

        var result = new InvoicesController(database).Pdf(invoice.Id, CreateService());

        var file = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", file.ContentType);
        Assert.Equal("Factura-V-2026-00001.pdf", file.FileDownloadName);
        Assert.True(file.FileContents.Length > 1000);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(file.FileContents, 0, 4));
    }

    [Fact]
    public void Pdf_ReturnsNotFoundForUnknownInvoice()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        using var database = CreateDatabase();

        var result = new InvoicesController(database).Pdf(999, CreateService());

        Assert.IsType<NotFoundResult>(result);
    }

    private static InvoicePdfService CreateService() =>
        new(Options.Create(new CompanyOptions
        {
            Name = "Empresa de prueba",
            TaxId = "B00000000",
            Address = "Dirección de prueba"
        }));

    private static AppDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
