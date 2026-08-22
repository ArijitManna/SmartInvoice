using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Application.Inventory.DTOs;

public record CreateStockTransferRequest(
    Guid FromWarehouseId,
    Guid ToWarehouseId,
    string? Notes,
    List<StockTransferItemRequest> Items
);

public record StockTransferItemRequest(
    Guid ProductId,
    Guid? BatchId,
    decimal Quantity
);

public record StockTransferResponse(
    Guid Id,
    string? TransferNumber,
    Guid FromWarehouseId,
    string FromWarehouseName,
    Guid ToWarehouseId,
    string ToWarehouseName,
    DateTime Date,
    StockTransferStatus Status,
    string? Notes,
    List<StockTransferItemResponse> Items
);

public record StockTransferItemResponse(
    Guid ProductId,
    string ProductName,
    Guid? BatchId,
    decimal Quantity
);
