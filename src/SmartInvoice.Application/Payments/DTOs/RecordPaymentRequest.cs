using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Application.Payments.DTOs;

public record RecordPaymentRequest(
    decimal Amount,
    PaymentMode PaymentMode,
    DateTime? PaymentDate,
    string? ReferenceNumber,
    string? Notes
);
