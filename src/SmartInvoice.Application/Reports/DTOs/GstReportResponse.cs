namespace SmartInvoice.Application.Reports.DTOs;

public record GstReportResponse(
    decimal TotalTaxCollected,
    decimal TotalCgst,
    decimal TotalSgst,
    decimal TotalIgst,
    List<GstReportItem> Items
);

public record GstReportItem(
    Guid InvoiceId,
    string InvoiceNumber,
    DateTime InvoiceDate,
    string CustomerName,
    string? CustomerGstin,
    decimal TaxableAmount,
    decimal CgstAmount,
    decimal SgstAmount,
    decimal IgstAmount,
    decimal TotalTax
);
