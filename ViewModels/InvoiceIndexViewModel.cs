using MiniErp.Models;

namespace MiniErp.ViewModels;

public class InvoiceIndexViewModel
{
    public IReadOnlyCollection<Invoice> Invoices { get; init; } = [];
    public IReadOnlyCollection<Customer> Customers { get; init; } = [];
    public IReadOnlyCollection<Supplier> Suppliers { get; init; } = [];
    public IReadOnlyCollection<int> Years { get; init; } = [];
    public InvoiceType? Type { get; init; }
    public InvoiceStatus? Status { get; init; }
    public int? Year { get; init; }
    public int? Month { get; init; }
    public int? CustomerId { get; init; }
    public int? SupplierId { get; init; }
    public decimal AccumulatedTaxBase { get; init; }
    public decimal AccumulatedTax { get; init; }
    public decimal AccumulatedTotal { get; init; }
    public decimal AccumulatedPaid { get; init; }
    public decimal AccumulatedOutstanding { get; init; }
}
