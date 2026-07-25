using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Application.Payments.DTOs;

public record PaymentResponse(
    Guid Id,
    Guid InvoiceId,
    decimal Amount,
    PaymentMode PaymentMode,
    DateTime PaymentDate,
    string? ReferenceNumber,
    string? Notes,
    bool IsRefund,
    DateTime CreatedAt
);
