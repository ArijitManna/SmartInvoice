using SmartInvoice.Domain.Common;

namespace SmartInvoice.Domain.Entities;

public class StockTransferItem : BaseEntity
{
    public Guid TransferId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? BatchId { get; set; }
    public decimal Quantity { get; set; }

    // Navigation
    public StockTransfer Transfer { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Batch? Batch { get; set; }
}
