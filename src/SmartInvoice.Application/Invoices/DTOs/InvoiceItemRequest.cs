namespace SmartInvoice.Application.Invoices.DTOs;

public record InvoiceItemRequest(
    Guid? ProductId,
    string Description,
    string? HsnSacCode,
    decimal Quantity,
    string Unit,
    decimal Rate,
    decimal DiscountPercentage,
    decimal TaxRate
);
