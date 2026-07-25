using Microsoft.EntityFrameworkCore;
using SmartInvoice.Application.Dashboard;
using SmartInvoice.Application.Dashboard.DTOs;
using SmartInvoice.Domain.Enums;
using SmartInvoice.Infrastructure.Persistence;

namespace SmartInvoice.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardResponse> GetDashboardAsync()
    {
        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Today's sales
        var todaySales = await _context.Payments
            .Where(p => p.PaymentDate >= today && !p.IsRefund)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        // Outstanding
        var outstandingAmount = await _context.Invoices
            .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled)
            .SumAsync(i => (decimal?)i.BalanceDue) ?? 0;

        // Invoice counts
        var invoicesThisMonth = await _context.Invoices
            .Where(i => i.CreatedAt >= monthStart)
            .CountAsync();

        var paidInvoices = await _context.Invoices
            .Where(i => i.Status == InvoiceStatus.Paid)
            .CountAsync();

        var pendingInvoices = await _context.Invoices
            .Where(i => i.Status == InvoiceStatus.Sent || i.Status == InvoiceStatus.PartiallyPaid)
            .CountAsync();

        var overdueInvoices = await _context.Invoices
            .Where(i => i.DueDate < today && i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled)
            .CountAsync();

        // GST collected this month
        var gstCollected = await _context.Invoices
            .Where(i => i.InvoiceDate >= monthStart && i.Status != InvoiceStatus.Cancelled)
            .SumAsync(i => (decimal?)i.TaxAmount) ?? 0;

        // Monthly revenue (last 12 months)
        var twelveMonthsAgo = today.AddMonths(-11);
        var startOfTwelveMonths = new DateTime(twelveMonthsAgo.Year, twelveMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var monthlyRevenue = await _context.Invoices
            .Where(i => i.InvoiceDate >= startOfTwelveMonths && i.Status != InvoiceStatus.Cancelled)
            .GroupBy(i => new { i.InvoiceDate.Year, i.InvoiceDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Amount = g.Sum(i => i.TotalAmount) })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();

        var monthlyRevenueItems = monthlyRevenue
            .Select(m => new MonthlyRevenueItem($"{m.Year}-{m.Month:D2}", m.Amount))
            .ToList();

        // Top 5 customers by revenue
        var topCustomers = await _context.Invoices
            .Where(i => i.Status != InvoiceStatus.Cancelled)
            .GroupBy(i => i.CustomerId)
            .Select(g => new { CustomerId = g.Key, TotalAmount = g.Sum(i => i.TotalAmount) })
            .OrderByDescending(c => c.TotalAmount)
            .Take(5)
            .ToListAsync();

        var topCustomerIds = topCustomers.Select(c => c.CustomerId).ToList();
        var customerNames = await _context.Customers
            .Where(c => topCustomerIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name);

        var topCustomerItems = topCustomers
            .Select(c => new TopCustomerItem(c.CustomerId, customerNames.GetValueOrDefault(c.CustomerId, "Unknown"), c.TotalAmount))
            .ToList();

        // Recent payments (last 10)
        var recentPayments = await _context.Payments
            .Where(p => !p.IsRefund)
            .OrderByDescending(p => p.PaymentDate)
            .Take(10)
            .Select(p => new RecentPaymentItem(
                p.InvoiceId, p.Invoice.InvoiceNumber, p.Invoice.Customer.Name,
                p.Amount, p.PaymentDate))
            .ToListAsync();

        // Upcoming due (next 10 unpaid invoices with due dates)
        var upcomingDue = await _context.Invoices
            .Include(i => i.Customer)
            .Where(i => i.DueDate != null && i.DueDate >= today
                && i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled)
            .OrderBy(i => i.DueDate)
            .Take(10)
            .Select(i => new UpcomingDueItem(
                i.Id, i.InvoiceNumber, i.Customer.Name,
                i.BalanceDue, i.DueDate!.Value))
            .ToListAsync();

        return new DashboardResponse(
            TodaySales: todaySales,
            OutstandingAmount: outstandingAmount,
            InvoicesCreatedThisMonth: invoicesThisMonth,
            PaidInvoices: paidInvoices,
            PendingInvoices: pendingInvoices,
            OverdueInvoices: overdueInvoices,
            GstCollected: gstCollected,
            MonthlyRevenue: monthlyRevenueItems,
            TopCustomers: topCustomerItems,
            RecentPayments: recentPayments,
            UpcomingDueInvoices: upcomingDue
        );
    }
}
