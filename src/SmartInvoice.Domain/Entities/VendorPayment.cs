using SmartInvoice.Domain.Common;
using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Domain.Entities;

public class VendorPayment : BaseEntity
{
    public Guid VendorId { get; set; }
    public Guid? PurchaseBillId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMode PaymentMode { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Vendor Vendor { get; set; } = null!;
    public PurchaseBill? PurchaseBill { get; set; }
}
