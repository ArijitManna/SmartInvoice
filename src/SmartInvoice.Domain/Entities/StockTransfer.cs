using SmartInvoice.Domain.Common;
using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Domain.Entities;

public class StockTransfer : BaseEntity
{
    public Guid FromWarehouseId { get; set; }
    public Guid ToWarehouseId { get; set; }
    public string? TransferNumber { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public StockTransferStatus Status { get; set; } = StockTransferStatus.Draft;
    public string? Notes { get; set; }

    // Navigation
    public Warehouse FromWarehouse { get; set; } = null!;
    public Warehouse ToWarehouse { get; set; } = null!;
    public ICollection<StockTransferItem> Items { get; set; } = [];
}
