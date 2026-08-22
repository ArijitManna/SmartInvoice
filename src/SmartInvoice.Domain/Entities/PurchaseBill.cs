using SmartInvoice.Domain.Common;
using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Domain.Entities;

public class PurchaseBill : BaseEntity
{
    public Guid VendorId { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public DateTime BillDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public PurchaseBillStatus Status { get; set; } = PurchaseBillStatus.Unpaid;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal BalanceDue { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Vendor Vendor { get; set; } = null!;
    public PurchaseOrder? PurchaseOrder { get; set; }
    public ICollection<PurchaseBillItem> Items { get; set; } = [];
    public ICollection<VendorPayment> Payments { get; set; } = [];
}
