using System.Data;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniErp.Data;
using MiniErp.Models;
using MiniErp.ViewModels;
using MiniErp.Services;

namespace MiniErp.Controllers;

public class InvoicesController(AppDbContext db) : Controller
{
    private static readonly HashSet<string> PaymentMethods = new(StringComparer.Ordinal)
    {
        "Transferencia",
        "Tarjeta",
        "Efectivo",
        "Domiciliación bancaria"
    };

    [HttpGet]
    public IActionResult Index(
        InvoiceType? type = null,
        InvoiceStatus? status = null,
        int? year = null,
        int? month = null,
        int? customerId = null,
        int? supplierId = null)
    {
        if (month is < 1 or > 12)
            return BadRequest();

        var query = db.Invoices
            .AsNoTracking()
            .Include(invoice => invoice.Payments)
            .Include(invoice => invoice.SalesOrder)
                .ThenInclude(order => order!.Customer)
            .Include(invoice => invoice.PurchaseOrder)
                .ThenInclude(order => order!.Supplier)
            .AsQueryable();

        if (type.HasValue)
            query = query.Where(invoice => invoice.Type == type);
        if (status.HasValue)
            query = query.Where(invoice => invoice.Status == status);
        if (year.HasValue)
            query = query.Where(invoice => invoice.IssueDate.Year == year);
        if (month.HasValue)
            query = query.Where(invoice => invoice.IssueDate.Month == month);
        if (customerId.HasValue)
            query = query.Where(invoice => invoice.SalesOrder != null && invoice.SalesOrder.CustomerId == customerId);
        if (supplierId.HasValue)
            query = query.Where(invoice => invoice.PurchaseOrder != null && invoice.PurchaseOrder.SupplierId == supplierId);

        var invoices = query
            .OrderByDescending(invoice => invoice.IssueDate)
            .ThenByDescending(invoice => invoice.Id)
            .ToArray();
        var accumulatedInvoices = invoices.Where(invoice => invoice.Status != InvoiceStatus.Cancelled).ToArray();

        return View(new InvoiceIndexViewModel
        {
            Invoices = invoices,
            Customers = db.Customers.AsNoTracking().OrderBy(customer => customer.Name).ToArray(),
            Suppliers = db.Suppliers.AsNoTracking().OrderBy(supplier => supplier.Name).ToArray(),
            Years = db.Invoices.AsNoTracking()
                .Select(invoice => invoice.IssueDate.Year)
                .Distinct()
                .OrderByDescending(value => value)
                .ToArray(),
            Type = type,
            Status = status,
            Year = year,
            Month = month,
            CustomerId = customerId,
            SupplierId = supplierId,
            AccumulatedTaxBase = accumulatedInvoices.Sum(invoice => invoice.TaxBase),
            AccumulatedTax = accumulatedInvoices.Sum(invoice => invoice.TaxAmount),
            AccumulatedTotal = accumulatedInvoices.Sum(invoice => invoice.Total),
            AccumulatedPaid = accumulatedInvoices.Sum(invoice => invoice.PaidAmount),
            AccumulatedOutstanding = accumulatedInvoices.Sum(invoice => invoice.Outstanding)
        });
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var invoice = db.Invoices.AsNoTracking().Include(item => item.Payments).FirstOrDefault(item => item.Id == id);
        return invoice is null ? NotFound() : View(invoice);
    }

    [HttpGet]
    public IActionResult Pdf(int id, [FromServices] InvoicePdfService pdfService)
    {
        var invoice = db.Invoices
            .AsNoTracking()
            .Include(item => item.Payments)
            .Include(item => item.SalesOrder)
                .ThenInclude(order => order!.Customer)
            .Include(item => item.SalesOrder)
                .ThenInclude(order => order!.Lines)
                    .ThenInclude(line => line.Product)
            .Include(item => item.PurchaseOrder)
                .ThenInclude(order => order!.Supplier)
            .Include(item => item.PurchaseOrder)
                .ThenInclude(order => order!.Lines)
                    .ThenInclude(line => line.Product)
            .FirstOrDefault(item => item.Id == id);

        if (invoice is null)
            return NotFound();

        var invalidCharacters = Path.GetInvalidFileNameChars();
        var invoiceNumber = string.Concat($"{invoice.Series}-{invoice.Number}".Where(character => !invalidCharacters.Contains(character)));
        return File(pdfService.Generate(invoice), "application/pdf", $"Factura-{invoiceNumber}.pdf");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult FromSale(int id, string series = "V", decimal taxRate = 21)
    {
        var order = db.SalesOrders.Include(item => item.Lines).FirstOrDefault(item => item.Id == id);
        if (order is null || order.Status != SalesOrderStatus.Completed)
            return BadRequest();
        return CreateInvoice(InvoiceType.Sale, series, taxRate, order.Total, id, null);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult FromPurchase(int id, string series = "C", decimal taxRate = 21)
    {
        var order = db.PurchaseOrders.Include(item => item.Lines).FirstOrDefault(item => item.Id == id);
        if (order is null || order.Status != PurchaseOrderStatus.Received)
            return BadRequest();
        return CreateInvoice(InvoiceType.Purchase, series, taxRate, order.Total, null, id);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Issue(int id)
    {
        using var transaction = db.Database.IsRelational() ? db.Database.BeginTransaction(IsolationLevel.Serializable) : null;
        var invoice = db.Invoices.Find(id);
        if (invoice is null)
            return NotFound();
        if (invoice.Status == InvoiceStatus.Draft)
        {
            invoice.Status = InvoiceStatus.Issued;
            db.SaveChanges();
        }
        transaction?.Commit();
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Pay(int id, string? amount, string? method)
    {
        if (!decimal.TryParse(amount, NumberStyles.Number, CultureInfo.CurrentCulture, out var parsedAmount))
            return BadRequest();

        return PayCore(id, parsedAmount, method);
    }

    [NonAction]
    public IActionResult Pay(int id, decimal amount, string? method) => PayCore(id, amount, method);

    private IActionResult PayCore(int id, decimal amount, string? method)
    {
        using var transaction = db.Database.IsRelational() ? db.Database.BeginTransaction(IsolationLevel.Serializable) : null;
        var invoice = db.Invoices.Include(item => item.Payments).FirstOrDefault(item => item.Id == id);
        if (invoice is null)
            return NotFound();

        method = method?.Trim();
        if (invoice.Status != InvoiceStatus.Issued || amount <= 0 || amount > invoice.Outstanding || method is null || !PaymentMethods.Contains(method))
            return BadRequest();

        var outstanding = invoice.Outstanding;
        invoice.Payments.Add(new Payment { Amount = amount, Method = method, Date = DateTime.Today });
        if (amount == outstanding)
            invoice.Status = InvoiceStatus.Paid;
        db.SaveChanges();
        transaction?.Commit();
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancel(int id)
    {
        using var transaction = db.Database.IsRelational() ? db.Database.BeginTransaction(IsolationLevel.Serializable) : null;
        var invoice = db.Invoices.Include(item => item.Payments).FirstOrDefault(item => item.Id == id);
        if (invoice is null)
            return NotFound();
        if (invoice.Payments.Count == 0 && invoice.Status is InvoiceStatus.Draft or InvoiceStatus.Issued)
        {
            invoice.Status = InvoiceStatus.Cancelled;
            db.SaveChanges();
        }
        transaction?.Commit();
        return RedirectToAction(nameof(Details), new { id });
    }

    private IActionResult CreateInvoice(InvoiceType type, string? series, decimal taxRate, decimal taxBase, int? salesOrderId, int? purchaseOrderId)
    {
        series = series?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(series) || series.Length > 10 || taxRate is < 0 or > 100 || taxBase < 0)
            return BadRequest();

        using var transaction = db.Database.IsRelational() ? db.Database.BeginTransaction(IsolationLevel.Serializable) : null;
        var existingInvoiceId = salesOrderId.HasValue
            ? db.Invoices.Where(invoice => invoice.SalesOrderId == salesOrderId).Select(invoice => (int?)invoice.Id).FirstOrDefault()
            : db.Invoices.Where(invoice => invoice.PurchaseOrderId == purchaseOrderId).Select(invoice => (int?)invoice.Id).FirstOrDefault();
        if (existingInvoiceId.HasValue)
            return RedirectToAction(nameof(Details), new { id = existingInvoiceId.Value });

        var yearPrefix = $"{DateTime.Today.Year}-";
        var numbers = db.Invoices.AsNoTracking()
            .Where(invoice => invoice.Series == series && invoice.Number.StartsWith(yearPrefix))
            .Select(invoice => invoice.Number)
            .ToArray();
        var nextNumber = numbers
            .Select(number => int.TryParse(number[yearPrefix.Length..], out var value) ? value : 0)
            .DefaultIfEmpty()
            .Max() + 1;

        var invoice = new Invoice
        {
            Type = type,
            Series = series,
            Number = $"{yearPrefix}{nextNumber:00000}",
            TaxBase = taxBase,
            TaxRate = taxRate,
            TaxAmount = decimal.Round(taxBase * taxRate / 100, 2),
            SalesOrderId = salesOrderId,
            PurchaseOrderId = purchaseOrderId
        };
        invoice.Total = invoice.TaxBase + invoice.TaxAmount;
        db.Add(invoice);
        db.SaveChanges();
        transaction?.Commit();
        return RedirectToAction(nameof(Details), new { id = invoice.Id });
    }
}
