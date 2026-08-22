using SmartInvoice.Domain.Common;
using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Domain.Entities;

public class StockEntry : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid? BatchId { get; set; }
    public decimal Quantity { get; set; }
    public StockEntryType Type { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? Notes { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;

    // Navigation
    public Product Product { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
    public Batch? Batch { get; set; }
}
