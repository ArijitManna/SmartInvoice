using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Application.Customers.DTOs;

public record CustomerLedgerEntryResponse(
    Guid Id,
    DateTime Date,
    LedgerEntryType Type,
    Guid? ReferenceId,
    string? ReferenceNumber,
    decimal Debit,
    decimal Credit,
    decimal RunningBalance,
    string? Notes
);
