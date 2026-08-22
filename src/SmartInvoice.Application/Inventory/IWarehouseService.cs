using SmartInvoice.Application.Common;
using SmartInvoice.Application.Inventory.DTOs;

namespace SmartInvoice.Application.Inventory;

public interface IWarehouseService
{
    Task<Result<WarehouseResponse>> CreateAsync(WarehouseRequest request);
    Task<IReadOnlyList<WarehouseResponse>> GetAllAsync();
    Task<Result<WarehouseResponse>> UpdateAsync(Guid id, WarehouseRequest request);
    Task<Result<bool>> DeleteAsync(Guid id);
}
