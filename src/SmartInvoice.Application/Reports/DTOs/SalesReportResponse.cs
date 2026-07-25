namespace SmartInvoice.Application.Reports.DTOs;

public record SalesReportResponse(
    decimal TotalSales,
    int TotalInvoices,
    decimal TotalTax,
    List<SalesReportItem> Items
);

public record SalesReportItem(
    Guid InvoiceId,
    string InvoiceNumber,
    DateTime InvoiceDate,
    string CustomerName,
    decimal SubTotal,
    decimal TaxAmount,
    decimal TotalAmount,
    string Status
);
