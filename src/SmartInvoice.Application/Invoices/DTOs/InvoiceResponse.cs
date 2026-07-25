using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Application.Invoices.DTOs;

public record InvoiceResponse(
    Guid Id,
    string InvoiceNumber,
    InvoiceType Type,
    InvoiceStatus Status,
    DateTime InvoiceDate,
    DateTime? DueDate,
    // Customer
    Guid CustomerId,
    string CustomerName,
    // Amounts
    decimal SubTotal,
    decimal DiscountPercentage,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal CgstAmount,
    decimal SgstAmount,
    decimal IgstAmount,
    decimal TotalAmount,
    decimal AmountPaid,
    decimal BalanceDue,
    string Currency,
    GstType GstType,
    // Additional
    string? Notes,
    string? TermsAndConditions,
    string? ReferenceNumber,
    // Items
    List<InvoiceItemResponse> Items,
    DateTime CreatedAt
);
