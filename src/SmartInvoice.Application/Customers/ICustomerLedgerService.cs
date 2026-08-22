using SmartInvoice.Application.Customers.DTOs;
using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Application.Customers;

public interface ICustomerLedgerService
{
    /// <summary>
    /// Add a ledger entry and update customer outstanding balance.
    /// </summary>
    Task AddEntryAsync(Guid customerId, LedgerEntryType type, Guid? referenceId, string? referenceNumber, decimal debit, decimal credit, string? notes = null);

    /// <summary>
    /// Get ledger entries for a customer (ordered by date desc).
    /// </summary>
    Task<IReadOnlyList<CustomerLedgerEntryResponse>> GetLedgerAsync(Guid customerId, DateTime? from = null, DateTime? to = null);

    /// <summary>
    /// Recalculate running balance for a customer from scratch.
    /// </summary>
    Task RecalculateBalanceAsync(Guid customerId);
}
