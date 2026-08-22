using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Application.Invoices.DTOs;

public record CreateRecurringInvoiceRequest(
    Guid CustomerId,
    RecurrenceFrequency Frequency,
    InvoiceType InvoiceType,
    decimal DiscountPercentage,
    string? Notes,
    string? TermsAndConditions,
    List<InvoiceItemRequest> Items
);

public record RecurringInvoiceResponse(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    RecurrenceFrequency Frequency,
    InvoiceType InvoiceType,
    DateTime NextGenerationDate,
    DateTime? LastGeneratedDate,
    bool IsActive,
    string? Notes,
    decimal DiscountPercentage,
    int ItemCount
);
