using SmartInvoice.Domain.Common;

namespace SmartInvoice.Domain.Entities;

public class PurchaseReturn : BaseEntity
{
    public Guid PurchaseBillId { get; set; }
    public Guid VendorId { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public string? Status { get; set; } = "Pending";

    // Navigation
    public PurchaseBill PurchaseBill { get; set; } = null!;
    public Vendor Vendor { get; set; } = null!;
}
