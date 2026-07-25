namespace SmartInvoice.Application.Reports.DTOs;

public record OutstandingReportResponse(
    decimal TotalOutstanding,
    int TotalInvoices,
    List<OutstandingItem> Items
);

public record OutstandingItem(
    Guid InvoiceId,
    string InvoiceNumber,
    DateTime InvoiceDate,
    DateTime? DueDate,
    string CustomerName,
    decimal TotalAmount,
    decimal AmountPaid,
    decimal BalanceDue,
    int DaysOverdue
);
