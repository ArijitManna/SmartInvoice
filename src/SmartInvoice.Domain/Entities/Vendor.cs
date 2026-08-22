using SmartInvoice.Domain.Common;
using SmartInvoice.Domain.ValueObjects;

namespace SmartInvoice.Domain.Entities;

public class Vendor : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? ContactPerson { get; set; }
    public string? Notes { get; set; }
    public decimal OutstandingBalance { get; set; }
    public decimal OpeningBalance { get; set; }

    // Value Objects
    public GstInfo GstInfo { get; set; } = new();
    public Address Address { get; set; } = new();
    public BankDetails BankDetails { get; set; } = new();

    // Navigation
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = [];
    public ICollection<PurchaseBill> PurchaseBills { get; set; } = [];
    public ICollection<VendorLedgerEntry> LedgerEntries { get; set; } = [];
    public ICollection<VendorPayment> Payments { get; set; } = [];
}
