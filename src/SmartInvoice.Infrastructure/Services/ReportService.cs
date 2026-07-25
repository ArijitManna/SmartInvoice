using Microsoft.EntityFrameworkCore;
using SmartInvoice.Application.Reports;
using SmartInvoice.Application.Reports.DTOs;
using SmartInvoice.Domain.Enums;
using SmartInvoice.Infrastructure.Persistence;

namespace SmartInvoice.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _context;

    public ReportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SalesReportResponse> GetSalesReportAsync(DateTime from, DateTime to, Guid? customerId = null)
    {
        var toEnd = to.Date.AddDays(1); // Include the entire 'to' day

        var query = _context.Invoices
            .Include(i => i.Customer)
            .Where(i => i.InvoiceDate >= from && i.InvoiceDate < toEnd && i.Status != InvoiceStatus.Cancelled);

        if (customerId.HasValue)
        {
            query = query.Where(i => i.CustomerId == customerId.Value);
        }

        var invoices = await query.OrderByDescending(i => i.InvoiceDate).ToListAsync();

        var items = invoices.Select(i => new SalesReportItem(
            i.Id, i.InvoiceNumber, i.InvoiceDate, i.Customer.Name,
            i.SubTotal, i.TaxAmount, i.TotalAmount, i.Status.ToString()
        )).ToList();

        return new SalesReportResponse(
            TotalSales: invoices.Sum(i => i.TotalAmount),
            TotalInvoices: invoices.Count,
            TotalTax: invoices.Sum(i => i.TaxAmount),
            Items: items
        );
    }

    public async Task<OutstandingReportResponse> GetOutstandingReportAsync()
    {
        var today = DateTime.UtcNow.Date;

        var invoices = await _context.Invoices
            .Include(i => i.Customer)
            .Where(i => i.BalanceDue > 0
                && i.Status != InvoiceStatus.Cancelled
                && i.Status != InvoiceStatus.Paid
                && i.Status != InvoiceStatus.Draft)
            .OrderBy(i => i.DueDate)
            .ToListAsync();

        var items = invoices.Select(i => new OutstandingItem(
            i.Id, i.InvoiceNumber, i.InvoiceDate, i.DueDate, i.Customer.Name,
            i.TotalAmount, i.AmountPaid, i.BalanceDue,
            i.DueDate.HasValue && i.DueDate.Value < today ? (today - i.DueDate.Value).Days : 0
        )).ToList();

        return new OutstandingReportResponse(
            TotalOutstanding: invoices.Sum(i => i.BalanceDue),
            TotalInvoices: invoices.Count,
            Items: items
        );
    }

    public async Task<GstReportResponse> GetGstReportAsync(DateTime from, DateTime to)
    {
        var toEnd = to.Date.AddDays(1);

        var invoices = await _context.Invoices
            .Include(i => i.Customer)
            .Where(i => i.InvoiceDate >= from && i.InvoiceDate < toEnd && i.Status != InvoiceStatus.Cancelled)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();

        var items = invoices.Select(i => new GstReportItem(
            i.Id, i.InvoiceNumber, i.InvoiceDate, i.Customer.Name,
            i.Customer.GstInfo.Gstin,
            i.SubTotal - i.DiscountAmount,
            i.CgstAmount, i.SgstAmount, i.IgstAmount, i.TaxAmount
        )).ToList();

        return new GstReportResponse(
            TotalTaxCollected: invoices.Sum(i => i.TaxAmount),
            TotalCgst: invoices.Sum(i => i.CgstAmount),
            TotalSgst: invoices.Sum(i => i.SgstAmount),
            TotalIgst: invoices.Sum(i => i.IgstAmount),
            Items: items
        );
    }
}
