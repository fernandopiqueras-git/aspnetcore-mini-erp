using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public void Pdf_GeneratesAnInvoiceForAPurchaseWithPayment()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        using var database = CreateDatabase();
        database.AddRange(
            new Supplier { Id = 1, Name = "Proveedor PDF", TaxId = "B12345674", Address = "Avenida Central 2" },
            new Warehouse { Id = 1, Code = "MAIN", Name = "Principal" },
            new Product { Id = 1, Sku = "ART-002", Name = "Artículo comprado", UnitPrice = 50 },
            new PurchaseOrder
            {
                Id = 1,
                Number = "PC-2026-0001",
                SupplierId = 1,
                WarehouseId = 1,
                Status = PurchaseOrderStatus.Received,
                Lines =
                [
                    new PurchaseOrderLine
                    {
                        ProductId = 1,
                        Quantity = 3,
                        UnitPrice = 50,
                        DiscountPercentage = 0
                    }
                ]
            });
        database.SaveChanges();
        var invoice = new Invoice
        {
            Type = InvoiceType.Purchase,
            Series = "C",
            Number = "2026-00001",
            Status = InvoiceStatus.Issued,
            PurchaseOrderId = 1,
            TaxBase = 150,
            TaxRate = 21,
            TaxAmount = 31.50m,
            Total = 181.50m,
            Payments = [new Payment { Amount = 50, Method = "Transferencia", Date = DateTime.Today }]
        };
        database.Add(invoice);
        database.SaveChanges();

        var result = new InvoicesController(database).Pdf(invoice.Id, CreateService());

        var file = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", file.ContentType);
        Assert.Equal("Factura-C-2026-00001.pdf", file.FileDownloadName);
        Assert.True(file.FileContents.Length > 1000);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(file.FileContents, 0, 4));
    }

    [Fact]
    public void Service_GeneratesAMultipageInvoice()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var order = new SalesOrder
        {
            Customer = new Customer { Name = "Cliente extenso", TaxId = "12345678Z" },
            Lines = Enumerable.Range(1, 80)
                .Select(index => new SalesOrderLine
                {
                    Product = new Product { Sku = $"ART-{index:000}", Name = $"Artículo número {index}" },
                    Quantity = 1,
                    UnitPrice = 10,
                    DiscountPercentage = 0
                })
                .ToArray()
        };
        var invoice = new Invoice
        {
            Type = InvoiceType.Sale,
            Series = "V",
            Number = "2026-00080",
            Status = InvoiceStatus.Issued,
            SalesOrder = order,
            TaxBase = 800,
            TaxRate = 21,
            TaxAmount = 168,
            Total = 968
        };

        var pdf = CreateService().Generate(invoice);

        Assert.True(pdf.Length > 5000);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(pdf, 0, 4));
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
        new(Microsoft.Extensions.Options.Options.Create(new CompanyOptions
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
