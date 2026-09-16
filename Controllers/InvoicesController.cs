using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniErp.Data;
using MiniErp.Models;

namespace MiniErp.Controllers;

public class InvoicesController(AppDbContext db) : Controller
{
    [HttpGet]
    public IActionResult Index(InvoiceType? type, InvoiceStatus? status)
    {
        var query = db.Invoices.AsNoTracking().Include(invoice => invoice.Payments).AsQueryable();
        if (type.HasValue)
            query = query.Where(invoice => invoice.Type == type);
        if (status.HasValue)
            query = query.Where(invoice => invoice.Status == status);
        return View(query.OrderByDescending(invoice => invoice.IssueDate).ToArray());
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var invoice = db.Invoices.AsNoTracking().Include(item => item.Payments).FirstOrDefault(item => item.Id == id);
        return invoice is null ? NotFound() : View(invoice);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult FromSale(int id, string series = "V", decimal taxRate = 21)
    {
        var order = db.SalesOrders.Include(item => item.Lines).FirstOrDefault(item => item.Id == id);
        if (order is null || order.Status != SalesOrderStatus.Completed)
            return BadRequest();
        if (db.Invoices.Any(invoice => invoice.SalesOrderId == id))
            return RedirectToAction(nameof(Index));
        return CreateInvoice(InvoiceType.Sale, series, taxRate, order.Total, id, null);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult FromPurchase(int id, string series = "C", decimal taxRate = 21)
    {
        var order = db.PurchaseOrders.Include(item => item.Lines).FirstOrDefault(item => item.Id == id);
        if (order is null || order.Status != PurchaseOrderStatus.Received)
            return BadRequest();
        if (db.Invoices.Any(invoice => invoice.PurchaseOrderId == id))
            return RedirectToAction(nameof(Index));
        return CreateInvoice(InvoiceType.Purchase, series, taxRate, order.Total, null, id);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Issue(int id)
    {
        var invoice = db.Invoices.Find(id);
        if (invoice is null)
            return NotFound();
        if (invoice.Status == InvoiceStatus.Draft)
        {
            invoice.Status = InvoiceStatus.Issued;
            db.SaveChanges();
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Pay(int id, decimal amount, string? method)
    {
        var invoice = db.Invoices.Include(item => item.Payments).FirstOrDefault(item => item.Id == id);
        if (invoice is null)
            return NotFound();

        method = method?.Trim();
        if (invoice.Status != InvoiceStatus.Issued || amount <= 0 || amount > invoice.Outstanding || string.IsNullOrWhiteSpace(method) || method.Length > 80)
            return BadRequest();

        invoice.Payments.Add(new Payment { Amount = amount, Method = method, Date = DateTime.Today });
        if (amount == invoice.Outstanding)
            invoice.Status = InvoiceStatus.Paid;
        db.SaveChanges();
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancel(int id)
    {
        var invoice = db.Invoices.Include(item => item.Payments).FirstOrDefault(item => item.Id == id);
        if (invoice is null)
            return NotFound();
        if (invoice.Payments.Count == 0 && invoice.Status is InvoiceStatus.Draft or InvoiceStatus.Issued)
        {
            invoice.Status = InvoiceStatus.Cancelled;
            db.SaveChanges();
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    private IActionResult CreateInvoice(InvoiceType type, string? series, decimal taxRate, decimal taxBase, int? salesOrderId, int? purchaseOrderId)
    {
        series = series?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(series) || series.Length > 10 || taxRate is < 0 or > 100 || taxBase < 0)
            return BadRequest();

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
        return RedirectToAction(nameof(Details), new { id = invoice.Id });
    }
}
