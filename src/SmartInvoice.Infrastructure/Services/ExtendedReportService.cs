using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SmartInvoice.Application.Reports;
using SmartInvoice.Application.Reports.DTOs;
using SmartInvoice.Domain.Enums;
using SmartInvoice.Infrastructure.Persistence;

namespace SmartInvoice.Infrastructure.Services;

public class ExtendedReportService : IExtendedReportService
{
    private readonly AppDbContext _context;

    public ExtendedReportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<HsnSummaryResponse> GetHsnSummaryAsync(DateTime from, DateTime to)
    {
        var toEnd = to.Date.AddDays(1);
        var items = await _context.InvoiceItems
            .Include(i => i.Invoice)
            .Where(i => i.Invoice.InvoiceDate >= from && i.Invoice.InvoiceDate < toEnd && i.Invoice.Status != InvoiceStatus.Cancelled)
            .GroupBy(i => new { Hsn = i.HsnSacCode ?? "N/A", i.Description })
            .Select(g => new HsnSummaryItem(
                g.Key.Hsn, g.Key.Description, (int)g.Sum(i => i.Quantity),
                g.Sum(i => i.Amount), 0, 0, 0, g.Sum(i => i.TaxAmount)))
            .ToListAsync();

        return new HsnSummaryResponse(items.Sum(i => i.TaxableValue), items.Sum(i => i.TotalTax), items);
    }

    public async Task<Gstr1Response> GetGstr1ReportAsync(DateTime from, DateTime to)
    {
        var toEnd = to.Date.AddDays(1);
        var invoices = await _context.Invoices
            .Include(i => i.Customer)
            .Where(i => i.InvoiceDate >= from && i.InvoiceDate < toEnd && i.Status != InvoiceStatus.Cancelled)
            .ToListAsync();

        var b2b = invoices
            .Where(i => !string.IsNullOrEmpty(i.Customer.GstInfo.Gstin))
            .Select(i => new Gstr1B2BEntry(
                i.Customer.GstInfo.Gstin!, i.Customer.Name, i.InvoiceNumber, i.InvoiceDate,
                i.SubTotal - i.DiscountAmount, i.CgstAmount, i.SgstAmount, i.IgstAmount, i.TaxAmount))
            .ToList();

        var b2c = invoices
            .Where(i => string.IsNullOrEmpty(i.Customer.GstInfo.Gstin))
            .Select(i => new Gstr1B2CEntry(i.InvoiceNumber, i.InvoiceDate, i.SubTotal - i.DiscountAmount, i.TaxAmount))
            .ToList();

        return new Gstr1Response(b2b, b2c, b2b.Sum(x => x.TaxableValue), b2c.Sum(x => x.TaxableValue), invoices.Sum(i => i.TaxAmount));
    }

    public async Task<ProfitLossResponse> GetProfitLossAsync(DateTime from, DateTime to)
    {
        var toEnd = to.Date.AddDays(1);

        // Revenue from paid invoices
        var revenue = await _context.Invoices
            .Where(i => i.InvoiceDate >= from && i.InvoiceDate < toEnd && i.Status != InvoiceStatus.Cancelled)
            .SumAsync(i => (decimal?)i.TotalAmount) ?? 0;

        // COGS (from purchase bills in period)
        var cogs = await _context.PurchaseBills
            .Where(b => b.BillDate >= from && b.BillDate < toEnd)
            .SumAsync(b => (decimal?)b.TotalAmount) ?? 0;

        // Expenses
        var expenses = await _context.Expenses
            .Include(e => e.Category)
            .Where(e => e.Date >= from && e.Date < toEnd)
            .ToListAsync();

        var totalExpenses = expenses.Sum(e => e.TotalAmount);
        var grossProfit = revenue - cogs;
        var netProfit = grossProfit - totalExpenses;

        var expenseBreakdown = expenses
            .GroupBy(e => e.Category.Name)
            .Select(g => new ProfitLossLineItem(g.Key, g.Sum(e => e.TotalAmount)))
            .OrderByDescending(x => x.Amount)
            .ToList();

        var revenueBreakdown = new List<ProfitLossLineItem>
        {
            new("Sales Revenue", revenue),
            new("Cost of Goods Sold", -cogs)
        };

        return new ProfitLossResponse(revenue, cogs, grossProfit, totalExpenses, netProfit, revenueBreakdown, expenseBreakdown);
    }

    public async Task<CashFlowResponse> GetCashFlowAsync(DateTime from, DateTime to)
    {
        var toEnd = to.Date.AddDays(1);

        // Inflows = payments received
        var payments = await _context.Payments
            .Where(p => p.PaymentDate >= from && p.PaymentDate < toEnd && !p.IsRefund)
            .ToListAsync();

        // Outflows = expenses + vendor payments
        var expenseTotal = await _context.Expenses
            .Where(e => e.Date >= from && e.Date < toEnd)
            .SumAsync(e => (decimal?)e.TotalAmount) ?? 0;

        var vendorPayments = await _context.VendorPayments
            .Where(vp => vp.PaymentDate >= from && vp.PaymentDate < toEnd)
            .SumAsync(vp => (decimal?)vp.Amount) ?? 0;

        var totalInflows = payments.Sum(p => p.Amount);
        var totalOutflows = expenseTotal + vendorPayments;

        // Group by month
        var months = Enumerable.Range(0, ((to.Year - from.Year) * 12 + to.Month - from.Month) + 1)
            .Select(i => from.AddMonths(i))
            .Select(d => new { Year = d.Year, Month = d.Month, Label = d.ToString("MMM yyyy") })
            .ToList();

        var periods = months.Select(m =>
        {
            var mStart = new DateTime(m.Year, m.Month, 1);
            var mEnd = mStart.AddMonths(1);
            var inflow = payments.Where(p => p.PaymentDate >= mStart && p.PaymentDate < mEnd).Sum(p => p.Amount);
            var outflow = expenseTotal / months.Count; // Simplified distribution
            return new CashFlowPeriod(m.Label, inflow, outflow, inflow - outflow);
        }).ToList();

        return new CashFlowResponse(totalInflows, totalOutflows, totalInflows - totalOutflows, periods);
    }

    public async Task<ProductReportResponse> GetProductReportAsync(DateTime from, DateTime to)
    {
        var toEnd = to.Date.AddDays(1);

        var items = await _context.InvoiceItems
            .Include(i => i.Invoice)
            .Include(i => i.Product)
            .Where(i => i.Invoice.InvoiceDate >= from && i.Invoice.InvoiceDate < toEnd && i.Invoice.Status != InvoiceStatus.Cancelled && i.ProductId != null)
            .GroupBy(i => new { i.ProductId, i.Product!.Name, i.Product.Sku })
            .Select(g => new ProductSalesItem(g.Key.ProductId!.Value, g.Key.Name, g.Key.Sku, (int)g.Sum(i => i.Quantity), g.Sum(i => i.Amount)))
            .ToListAsync();

        var topSelling = items.OrderByDescending(i => i.Revenue).Take(10).ToList();
        var slowMoving = items.OrderBy(i => i.QuantitySold).Take(10).ToList();

        return new ProductReportResponse(topSelling, slowMoving, items.Sum(i => i.Revenue));
    }

    public async Task<YearlyRevenueResponse> GetYearlyRevenueAsync(int year)
    {
        var yearStart = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var yearEnd = new DateTime(year + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var monthly = await _context.Invoices
            .Where(i => i.InvoiceDate >= yearStart && i.InvoiceDate < yearEnd && i.Status != InvoiceStatus.Cancelled)
            .GroupBy(i => i.InvoiceDate.Month)
            .Select(g => new { Month = g.Key, Amount = g.Sum(i => i.TotalAmount) })
            .ToListAsync();

        var months = Enumerable.Range(1, 12).Select(m =>
        {
            var data = monthly.FirstOrDefault(x => x.Month == m);
            return new MonthlyAmount(m, CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(m), data?.Amount ?? 0);
        }).ToList();

        return new YearlyRevenueResponse(year, months.Sum(m => m.Amount), months);
    }

    public async Task<TaxCollectedResponse> GetTaxCollectedAsync(DateTime from, DateTime to)
    {
        var toEnd = to.Date.AddDays(1);

        var invoices = await _context.Invoices
            .Where(i => i.InvoiceDate >= from && i.InvoiceDate < toEnd && i.Status != InvoiceStatus.Cancelled)
            .ToListAsync();

        var totalCgst = invoices.Sum(i => i.CgstAmount);
        var totalSgst = invoices.Sum(i => i.SgstAmount);
        var totalIgst = invoices.Sum(i => i.IgstAmount);

        var byPeriod = invoices
            .GroupBy(i => $"{i.InvoiceDate.Year}-{i.InvoiceDate.Month:D2}")
            .Select(g => new TaxPeriodItem(g.Key, g.Sum(i => i.CgstAmount), g.Sum(i => i.SgstAmount), g.Sum(i => i.IgstAmount), g.Sum(i => i.TaxAmount)))
            .OrderBy(x => x.Period)
            .ToList();

        return new TaxCollectedResponse(totalCgst + totalSgst + totalIgst, totalCgst, totalSgst, totalIgst, byPeriod);
    }
}
