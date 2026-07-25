namespace SmartInvoice.Application.Invoices.DTOs;

public record InvoiceItemResponse(
    Guid Id,
    Guid? ProductId,
    string? ProductName,
    string Description,
    string? HsnSacCode,
    decimal Quantity,
    string Unit,
    decimal Rate,
    decimal DiscountPercentage,
    decimal DiscountAmount,
    decimal TaxRate,
    decimal TaxAmount,
    decimal Amount
);
