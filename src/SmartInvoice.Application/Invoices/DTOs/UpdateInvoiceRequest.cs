namespace SmartInvoice.Application.Invoices.DTOs;

public record UpdateInvoiceRequest(
    Guid CustomerId,
    DateTime? DueDate,
    decimal DiscountPercentage,
    string? Notes,
    string? TermsAndConditions,
    string? ReferenceNumber,
    List<InvoiceItemRequest> Items
);
