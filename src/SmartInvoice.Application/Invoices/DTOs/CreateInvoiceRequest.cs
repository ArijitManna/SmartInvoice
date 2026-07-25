using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Application.Invoices.DTOs;

public record CreateInvoiceRequest(
    Guid CustomerId,
    InvoiceType Type,
    DateTime? DueDate,
    decimal DiscountPercentage,
    string? Notes,
    string? TermsAndConditions,
    string? ReferenceNumber,
    List<InvoiceItemRequest> Items
);
