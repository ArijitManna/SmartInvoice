namespace SmartInvoice.Application.Dashboard.DTOs;

public record DashboardResponse(
    decimal TodaySales,
    decimal OutstandingAmount,
    int InvoicesCreatedThisMonth,
    int PaidInvoices,
    int PendingInvoices,
    int OverdueInvoices,
    decimal GstCollected,
    List<MonthlyRevenueItem> MonthlyRevenue,
    List<TopCustomerItem> TopCustomers,
    List<RecentPaymentItem> RecentPayments,
    List<UpcomingDueItem> UpcomingDueInvoices
);

public record MonthlyRevenueItem(string Month, decimal Amount);
public record TopCustomerItem(Guid CustomerId, string Name, decimal TotalAmount);
public record RecentPaymentItem(Guid InvoiceId, string InvoiceNumber, string CustomerName, decimal Amount, DateTime PaymentDate);
public record UpcomingDueItem(Guid InvoiceId, string InvoiceNumber, string CustomerName, decimal BalanceDue, DateTime DueDate);
