using System.Globalization;
using Microsoft.Extensions.Options;
using MiniErp.Models;
using MiniErp.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MiniErp.Services;

public class InvoicePdfService(IOptions<CompanyOptions> companyOptions)
{
    public byte[] Generate(Invoice invoice)
    {
        var company = companyOptions.Value;
        var english = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en";
        var culture = CultureInfo.CurrentCulture;
        var partyName = invoice.SalesOrder?.Customer?.Name ?? invoice.PurchaseOrder?.Supplier?.Name ?? string.Empty;
        var partyTaxId = invoice.SalesOrder?.Customer?.TaxId ?? invoice.PurchaseOrder?.Supplier?.TaxId ?? string.Empty;
        var partyAddress = invoice.SalesOrder?.Customer?.Address ?? invoice.PurchaseOrder?.Supplier?.Address ?? string.Empty;
        var lines = invoice.Type == InvoiceType.Sale
            ? invoice.SalesOrder?.Lines.Select(line => new InvoiceLine(
                line.Product.Sku,
                line.Product.Name,
                line.Quantity,
                line.UnitPrice,
                line.DiscountPercentage,
                line.LineTotal)).ToArray() ?? []
            : invoice.PurchaseOrder?.Lines.Select(line => new InvoiceLine(
                line.Product.Sku,
                line.Product.Name,
                line.Quantity,
                line.UnitPrice,
                line.DiscountPercentage,
                line.LineTotal)).ToArray() ?? [];

        string Text(string spanish, string englishText) => english ? englishText : spanish;
        string Money(decimal value) => $"{value.ToString("N2", culture)} €";
        string Number(decimal value) => value.ToString("N3", culture);
        string Status() => invoice.Status switch
        {
            InvoiceStatus.Draft => Text("BORRADOR", "DRAFT"),
            InvoiceStatus.Issued => Text("EMITIDA", "ISSUED"),
            InvoiceStatus.Paid => Text("PAGADA", "PAID"),
            InvoiceStatus.Cancelled => Text("CANCELADA", "CANCELLED"),
            _ => invoice.Status.ToString().ToUpperInvariant()
        };

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(18, Unit.Millimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(style => style.FontSize(9).FontColor(Colors.Grey.Darken4));

                page.Header().Column(header =>
                {
                    header.Item().Row(row =>
                    {
                        row.RelativeItem().Element(container => ComposeCompany(container, company));
                        row.ConstantItem(220).AlignRight().Column(column =>
                        {
                            column.Item().Text(invoice.Type == InvoiceType.Sale
                                ? Text("FACTURA DE VENTA", "SALES INVOICE")
                                : Text("FACTURA DE COMPRA", "PURCHASE INVOICE")).FontSize(22).Bold().FontColor(Colors.Blue.Darken2);
                            column.Item().Text($"{invoice.Series}-{invoice.Number}").FontSize(13).SemiBold();
                            column.Item().PaddingTop(4).Text(Status()).Bold().FontColor(invoice.Status == InvoiceStatus.Cancelled ? Colors.Red.Darken2 : Colors.Blue.Darken2);
                        });
                    });
                    header.Item().PaddingVertical(12).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().Column(content =>
                {
                    content.Spacing(14);
                    content.Item().Row(row =>
                    {
                        row.RelativeItem().Column(column =>
                        {
                            column.Item().Text(invoice.Type == InvoiceType.Sale
                                ? Text("CLIENTE", "CUSTOMER")
                                : Text("PROVEEDOR", "SUPPLIER")).FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                            column.Item().PaddingTop(4).Text(partyName).FontSize(11).Bold();
                            column.Item().Text(partyTaxId);
                            if (!string.IsNullOrWhiteSpace(partyAddress))
                                column.Item().Text(partyAddress);
                        });
                        row.ConstantItem(210).Column(column =>
                        {
                            DetailRow(column, Text("Fecha", "Issue date"), invoice.IssueDate.ToString("d", culture));
                            DetailRow(column, Text("Vencimiento", "Due date"), invoice.DueDate.ToString("d", culture));
                            DetailRow(column, Text("Tipo", "Type"), invoice.Type == InvoiceType.Sale ? Text("Venta", "Sale") : Text("Compra", "Purchase"));
                            var orderNumber = invoice.SalesOrder?.Number ?? invoice.PurchaseOrder?.Number;
                            if (!string.IsNullOrWhiteSpace(orderNumber))
                                DetailRow(column, Text("Pedido", "Order"), orderNumber);
                        });
                    });

                    content.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(62);
                            columns.RelativeColumn(3);
                            columns.ConstantColumn(55);
                            columns.ConstantColumn(70);
                            columns.ConstantColumn(55);
                            columns.ConstantColumn(75);
                        });
                        table.Header(header =>
                        {
                            header.Cell().Element(cell => HeaderCell(cell, "SKU"));
                            header.Cell().Element(cell => HeaderCell(cell, Text("Artículo", "Product")));
                            header.Cell().Element(cell => HeaderCell(cell, Text("Cantidad", "Quantity"), true));
                            header.Cell().Element(cell => HeaderCell(cell, Text("Precio", "Price"), true));
                            header.Cell().Element(cell => HeaderCell(cell, Text("Dto.", "Disc."), true));
                            header.Cell().Element(cell => HeaderCell(cell, Text("Importe", "Amount"), true));
                        });
                        foreach (var line in lines)
                        {
                            table.Cell().Element(cell => BodyCell(cell, line.Sku));
                            table.Cell().Element(cell => BodyCell(cell, line.Name));
                            table.Cell().Element(cell => BodyCell(cell, Number(line.Quantity), true));
                            table.Cell().Element(cell => BodyCell(cell, Money(line.UnitPrice), true));
                            table.Cell().Element(cell => BodyCell(cell, $"{line.DiscountPercentage.ToString("N2", culture)} %", true));
                            table.Cell().Element(cell => BodyCell(cell, Money(line.Total), true));
                        }
                    });

                    content.Item().AlignRight().Width(250).Column(summary =>
                    {
                        SummaryRow(summary, Text("Base imponible", "Taxable base"), Money(invoice.TaxBase));
                        SummaryRow(summary, $"{Text("IVA", "Tax")} ({invoice.TaxRate.ToString("N2", culture)} %)", Money(invoice.TaxAmount));
                        summary.Item().PaddingTop(5).BorderTop(1).BorderColor(Colors.Grey.Lighten1);
                        SummaryRow(summary, Text("TOTAL", "TOTAL"), Money(invoice.Total), true);
                        SummaryRow(summary, Text("Pagado", "Paid"), Money(invoice.PaidAmount));
                        SummaryRow(summary, Text("Pendiente", "Outstanding"), Money(invoice.Outstanding), true);
                    });

                    if (invoice.Payments.Count > 0)
                    {
                        content.Item().PaddingTop(4).Text(Text("Cobros/Pagos", "Receipts/Payments")).FontSize(11).Bold();
                        content.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            table.Header(header =>
                            {
                                header.Cell().Element(cell => HeaderCell(cell, Text("Fecha", "Date")));
                                header.Cell().Element(cell => HeaderCell(cell, Text("Método", "Method")));
                                header.Cell().Element(cell => HeaderCell(cell, Text("Importe", "Amount"), true));
                            });
                            foreach (var payment in invoice.Payments.OrderBy(payment => payment.Date))
                            {
                                table.Cell().Element(cell => BodyCell(cell, payment.Date.ToString("d", culture)));
                                table.Cell().Element(cell => BodyCell(cell, payment.Method));
                                table.Cell().Element(cell => BodyCell(cell, Money(payment.Amount), true));
                            }
                        });
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span(Text("Página ", "Page "));
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                }).FontSize(8).FontColor(Colors.Grey.Darken1);
            });
        }).GeneratePdf();
    }

    private static void ComposeCompany(IContainer container, CompanyOptions company)
    {
        container.Row(row =>
        {
            if (!string.IsNullOrWhiteSpace(company.LogoPath) && File.Exists(company.LogoPath))
                row.ConstantItem(80).Height(45).PaddingRight(10).Image(company.LogoPath).FitArea();

            row.RelativeItem().Column(column =>
            {
                column.Item().Text(company.Name).FontSize(15).Bold().FontColor(Colors.Blue.Darken2);
                if (!string.IsNullOrWhiteSpace(company.TaxId))
                    column.Item().Text(company.TaxId);
                if (!string.IsNullOrWhiteSpace(company.Address))
                    column.Item().Text(company.Address);
                if (!string.IsNullOrWhiteSpace(company.Email))
                    column.Item().Text(company.Email);
                if (!string.IsNullOrWhiteSpace(company.Phone))
                    column.Item().Text(company.Phone);
            });
        });
    }

    private static void DetailRow(ColumnDescriptor column, string label, string value)
    {
        column.Item().PaddingBottom(3).Row(row =>
        {
            row.RelativeItem().Text(label).FontColor(Colors.Grey.Darken1);
            row.RelativeItem().AlignRight().Text(value).SemiBold();
        });
    }

    private static void HeaderCell(IContainer container, string text, bool alignRight = false)
    {
        var cell = container.Background(Colors.Blue.Darken2).Padding(6);
        if (alignRight)
            cell = cell.AlignRight();
        cell.Text(text).FontColor(Colors.White).Bold();
    }

    private static void BodyCell(IContainer container, string text, bool alignRight = false)
    {
        var cell = container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(6).PaddingHorizontal(4);
        if (alignRight)
            cell = cell.AlignRight();
        cell.Text(text);
    }

    private static void SummaryRow(ColumnDescriptor column, string label, string value, bool bold = false)
    {
        column.Item().PaddingVertical(3).Row(row =>
        {
            var labelText = row.RelativeItem().Text(label);
            var valueText = row.AutoItem().Text(value);
            if (bold)
            {
                labelText.Bold();
                valueText.Bold().FontSize(11);
            }
        });
    }

    private sealed record InvoiceLine(
        string Sku,
        string Name,
        decimal Quantity,
        decimal UnitPrice,
        decimal DiscountPercentage,
        decimal Total);
}
