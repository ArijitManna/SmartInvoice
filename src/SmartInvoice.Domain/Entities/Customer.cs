using SmartInvoice.Domain.Common;
using SmartInvoice.Domain.ValueObjects;

namespace SmartInvoice.Domain.Entities;

public class Customer : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? ContactPerson { get; set; }
    public string? Notes { get; set; }

    // Financial
    public decimal CreditLimit { get; set; }
    public decimal OutstandingBalance { get; set; }
    public decimal OpeningBalance { get; set; }

    // Value Objects (kept for backward compatibility with existing invoices)
    public GstInfo GstInfo { get; set; } = new();
    public Address BillingAddress { get; set; } = new();
    public Address? ShippingAddress { get; set; }

    // Navigation
    public ICollection<Invoice> Invoices { get; set; } = [];
    public ICollection<CustomerAddress> Addresses { get; set; } = [];
    public ICollection<CustomerLedgerEntry> LedgerEntries { get; set; } = [];

    /// <summary>
    /// Check if a new invoice amount would exceed the credit limit.
    /// CreditLimit of 0 means unlimited.
    /// </summary>
    public bool WouldExceedCreditLimit(decimal invoiceAmount)
    {
        if (CreditLimit <= 0) return false;
        return (OutstandingBalance + invoiceAmount) > CreditLimit;
    }
}
