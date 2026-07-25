using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Application.Invoices.DTOs;

/// <summary>
/// Lightweight response for invoice list views (no items).
/// </summary>
public record InvoiceListResponse(
    Guid Id,
    string InvoiceNumber,
    InvoiceType Type,
    InvoiceStatus Status,
    DateTime InvoiceDate,
    DateTime? DueDate,
    Guid CustomerId,
    string CustomerName,
    decimal TotalAmount,
    decimal BalanceDue,
    string Currency,
    DateTime CreatedAt
);
