using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartInvoice.Application.Invoices;
using SmartInvoice.Domain.Entities;
using SmartInvoice.Domain.Enums;
using SmartInvoice.Infrastructure.Persistence;

namespace SmartInvoice.Infrastructure.Services;

public class InvoicePdfService : IInvoicePdfService
{
    private readonly AppDbContext _context;

    public InvoicePdfService(AppDbContext context)
    {
        _context = context;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateAsync(Guid invoiceId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Items)
                .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(i => i.Id == invoiceId)
            ?? throw new InvalidOperationException("Invoice not found.");

        var company = await _context.Companies
            .IgnoreQueryFilters()
            .FirstAsync(c => c.Id == invoice.CompanyId && !c.IsDeleted);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, company, invoice));
                page.Content().Element(c => ComposeContent(c, company, invoice));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, Company company, Invoice invoice)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text(company.Name).FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                if (!string.IsNullOrEmpty(company.Address.Street))
                    col.Item().Text($"{company.Address.Street}");
                col.Item().Text($"{company.Address.City}, {company.Address.State} {company.Address.PostalCode}");
                if (!string.IsNullOrEmpty(company.Phone))
                    col.Item().Text($"Phone: {company.Phone}");
                if (!string.IsNullOrEmpty(company.Email))
                    col.Item().Text($"Email: {company.Email}");
                if (!string.IsNullOrEmpty(company.GstInfo.Gstin))
                    col.Item().Text($"GSTIN: {company.GstInfo.Gstin}").FontSize(9);
            });

            row.ConstantItem(200).Column(col =>
            {
                col.Item().AlignRight().Text(GetInvoiceTypeLabel(invoice.Type)).FontSize(16).Bold();
                col.Item().AlignRight().Text($"# {invoice.InvoiceNumber}").FontSize(12);
                col.Item().AlignRight().Text($"Date: {invoice.InvoiceDate:dd MMM yyyy}");
                if (invoice.DueDate.HasValue)
                    col.Item().AlignRight().Text($"Due: {invoice.DueDate:dd MMM yyyy}");
                col.Item().AlignRight().PaddingTop(5)
                    .Text(invoice.Status.ToString())
                    .FontSize(11).Bold()
                    .FontColor(GetStatusColor(invoice.Status));
            });
        });
    }

    private static void ComposeContent(IContainer container, Company company, Invoice invoice)
    {
        container.PaddingVertical(15).Column(col =>
        {
            // Bill To
            col.Item().PaddingBottom(10).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Bill To:").Bold();
                    c.Item().Text(invoice.Customer.Name).FontSize(11).Bold();
                    if (!string.IsNullOrEmpty(invoice.Customer.BillingAddress.Street))
                        c.Item().Text(invoice.Customer.BillingAddress.Street);
                    c.Item().Text($"{invoice.Customer.BillingAddress.City}, {invoice.Customer.BillingAddress.State} {invoice.Customer.BillingAddress.PostalCode}");
                    if (!string.IsNullOrEmpty(invoice.Customer.GstInfo.Gstin))
                        c.Item().Text($"GSTIN: {invoice.Customer.GstInfo.Gstin}").FontSize(9);
                });
            });

            // Items Table
            col.Item().Element(c => ComposeTable(c, invoice));

            // Totals
            col.Item().PaddingTop(10).AlignRight().Width(250).Element(c => ComposeTotals(c, invoice));

            // Bank Details
            if (!string.IsNullOrEmpty(company.BankDetails.BankName))
            {
                col.Item().PaddingTop(20).Element(c => ComposeBankDetails(c, company));
            }

            // Notes
            if (!string.IsNullOrEmpty(invoice.Notes))
            {
                col.Item().PaddingTop(15).Column(c =>
                {
                    c.Item().Text("Notes:").Bold();
                    c.Item().Text(invoice.Notes);
                });
            }

            // Terms
            if (!string.IsNullOrEmpty(invoice.TermsAndConditions))
            {
                col.Item().PaddingTop(10).Column(c =>
                {
                    c.Item().Text("Terms & Conditions:").Bold();
                    c.Item().Text(invoice.TermsAndConditions).FontSize(8);
                });
            }
        });
    }

    private static void ComposeTable(IContainer container, Invoice invoice)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(30);   // #
                cols.RelativeColumn(3);     // Description
                cols.ConstantColumn(50);   // Qty
                cols.ConstantColumn(70);   // Rate
                cols.ConstantColumn(50);   // Tax%
                cols.ConstantColumn(60);   // Tax Amt
                cols.ConstantColumn(80);   // Amount
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("#").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Description").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight().Text("Qty").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight().Text("Rate").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight().Text("Tax%").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight().Text("Tax").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight().Text("Amount").FontColor(Colors.White).Bold();
            });

            int index = 1;
            foreach (var item in invoice.Items)
            {
                var bgColor = index % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                table.Cell().Background(bgColor).Padding(4).Text(index.ToString());
                table.Cell().Background(bgColor).Padding(4).Text(item.Description);
                table.Cell().Background(bgColor).Padding(4).AlignRight().Text($"{item.Quantity:G}");
                table.Cell().Background(bgColor).Padding(4).AlignRight().Text($"{item.Rate:N2}");
                table.Cell().Background(bgColor).Padding(4).AlignRight().Text($"{item.TaxRate:G}%");
                table.Cell().Background(bgColor).Padding(4).AlignRight().Text($"{item.TaxAmount:N2}");
                table.Cell().Background(bgColor).Padding(4).AlignRight().Text($"{item.Amount:N2}");

                index++;
            }
        });
    }

    private static void ComposeTotals(IContainer container, Invoice invoice)
    {
        container.Column(col =>
        {
            void AddRow(string label, string value, bool bold = false)
            {
                col.Item().Row(row =>
                {
                    var text = row.RelativeItem().AlignRight().PaddingRight(10).Text(label);
                    if (bold) text.Bold();
                    var val = row.ConstantItem(100).AlignRight().Text(value);
                    if (bold) val.Bold();
                });
            }

            AddRow("Sub Total:", $"{invoice.SubTotal:N2}");

            if (invoice.DiscountAmount > 0)
                AddRow($"Discount ({invoice.DiscountPercentage}%):", $"-{invoice.DiscountAmount:N2}");

            if (invoice.GstType == GstType.IntraState)
            {
                AddRow("CGST:", $"{invoice.CgstAmount:N2}");
                AddRow("SGST:", $"{invoice.SgstAmount:N2}");
            }
            else
            {
                AddRow("IGST:", $"{invoice.IgstAmount:N2}");
            }

            col.Item().PaddingVertical(3).LineHorizontal(1);
            AddRow("Total:", $"{invoice.Currency} {invoice.TotalAmount:N2}", bold: true);

            if (invoice.AmountPaid > 0)
            {
                AddRow("Paid:", $"-{invoice.AmountPaid:N2}");
                AddRow("Balance Due:", $"{invoice.Currency} {invoice.BalanceDue:N2}", bold: true);
            }
        });
    }

    private static void ComposeBankDetails(IContainer container, Company company)
    {
        container.Background(Colors.Grey.Lighten4).Padding(10).Column(col =>
        {
            col.Item().Text("Bank Details:").Bold();
            col.Item().Text($"Bank: {company.BankDetails.BankName}");
            col.Item().Text($"A/C: {company.BankDetails.AccountNumber}");
            col.Item().Text($"IFSC: {company.BankDetails.IfscCode}");
            col.Item().Text($"Name: {company.BankDetails.AccountHolderName}");
            if (!string.IsNullOrEmpty(company.BankDetails.UpiId))
                col.Item().Text($"UPI: {company.BankDetails.UpiId}");
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span("Page ");
            text.CurrentPageNumber();
            text.Span(" of ");
            text.TotalPages();
        });
    }

    private static string GetInvoiceTypeLabel(InvoiceType type) => type switch
    {
        InvoiceType.Regular => "INVOICE",
        InvoiceType.GstInvoice => "TAX INVOICE",
        InvoiceType.Proforma => "PROFORMA INVOICE",
        InvoiceType.CreditNote => "CREDIT NOTE",
        InvoiceType.DebitNote => "DEBIT NOTE",
        InvoiceType.Quotation => "QUOTATION",
        InvoiceType.Estimate => "ESTIMATE",
        _ => "INVOICE"
    };

    private static string GetStatusColor(InvoiceStatus status) => status switch
    {
        InvoiceStatus.Paid => Colors.Green.Darken2,
        InvoiceStatus.PartiallyPaid => Colors.Orange.Darken2,
        InvoiceStatus.Overdue => Colors.Red.Darken2,
        InvoiceStatus.Cancelled => Colors.Grey.Darken1,
        _ => Colors.Blue.Darken2
    };
}
