namespace SmartInvoice.Application.Inventory.DTOs;

public record StockLevelResponse(
    Guid ProductId,
    string ProductName,
    string? Sku,
    Guid WarehouseId,
    string WarehouseName,
    decimal CurrentStock,
    int LowStockThreshold,
    bool IsLowStock
);

public record StockAdjustmentRequest(
    Guid ProductId,
    Guid WarehouseId,
    decimal Quantity,
    string Reason
);

public record StockSummaryResponse(
    Guid ProductId,
    string ProductName,
    string? Sku,
    string? Brand,
    decimal TotalStock,
    decimal PurchasePrice,
    decimal StockValue,
    int LowStockThreshold,
    bool IsLowStock
);
