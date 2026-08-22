using SmartInvoice.Application.Inventory.DTOs;
using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Application.Inventory;

public interface IInventoryService
{
    /// <summary>
    /// Get current stock levels per product per warehouse.
    /// </summary>
    Task<IReadOnlyList<StockLevelResponse>> GetStockLevelsAsync(Guid? productId = null, Guid? warehouseId = null);

    /// <summary>
    /// Get stock summary (total across all warehouses) per product.
    /// </summary>
    Task<IReadOnlyList<StockSummaryResponse>> GetStockSummaryAsync();

    /// <summary>
    /// Record a stock entry (in/out/adjustment).
    /// </summary>
    Task RecordStockEntryAsync(Guid productId, Guid warehouseId, decimal quantity, StockEntryType type, string? referenceType = null, Guid? referenceId = null, Guid? batchId = null, string? notes = null);

    /// <summary>
    /// Adjust stock (positive = add, negative = remove).
    /// </summary>
    Task AdjustStockAsync(StockAdjustmentRequest request);

    /// <summary>
    /// Get products with stock below their threshold.
    /// </summary>
    Task<IReadOnlyList<StockSummaryResponse>> GetLowStockAlertsAsync();
}
