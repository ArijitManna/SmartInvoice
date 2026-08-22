using SmartInvoice.Domain.Common;
using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Domain.Entities;

public class PurchaseOrder : BaseEntity
{
    public Guid VendorId { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public string? Terms { get; set; }

    // Navigation
    public Vendor Vendor { get; set; } = null!;
    public ICollection<PurchaseOrderItem> Items { get; set; } = [];
}
