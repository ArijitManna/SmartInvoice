using SmartInvoice.Application.Common;
using SmartInvoice.Application.Purchase.DTOs;

namespace SmartInvoice.Application.Purchase;

public interface IVendorService
{
    Task<Result<VendorResponse>> CreateAsync(VendorRequest request);
    Task<Result<VendorResponse>> GetByIdAsync(Guid id);
    Task<PagedResult<VendorResponse>> GetAllAsync(int page = 1, int pageSize = 20, string? search = null);
    Task<Result<VendorResponse>> UpdateAsync(Guid id, VendorRequest request);
    Task<Result<bool>> DeleteAsync(Guid id);
}
