using SmartInvoice.Application.Common;
using SmartInvoice.Application.Inventory.DTOs;

namespace SmartInvoice.Application.Inventory;

public interface IStockTransferService
{
    Task<Result<StockTransferResponse>> CreateAsync(CreateStockTransferRequest request);
    Task<IReadOnlyList<StockTransferResponse>> GetAllAsync();
    Task<Result<StockTransferResponse>> CompleteAsync(Guid transferId);
    Task<Result<bool>> CancelAsync(Guid transferId);
}
