using SmartInvoice.Domain.Common;
using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Domain.Entities;

public class VendorLedgerEntry : BaseEntity
{
    public Guid VendorId { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public LedgerEntryType Type { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceNumber { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal RunningBalance { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Vendor Vendor { get; set; } = null!;
}
