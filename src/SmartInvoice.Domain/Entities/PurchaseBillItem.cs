using SmartInvoice.Domain.Common;

namespace SmartInvoice.Domain.Entities;

public class PurchaseBillItem : BaseEntity
{
    public Guid PurchaseBillId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? BatchId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "Nos";
    public decimal Rate { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Amount { get; set; }

    // Navigation
    public PurchaseBill PurchaseBill { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Batch? Batch { get; set; }
}
