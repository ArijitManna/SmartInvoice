using SmartInvoice.Application.Common;
using SmartInvoice.Application.Products.DTOs;

namespace SmartInvoice.Application.Products;

public interface IProductService
{
    Task<Result<ProductResponse>> CreateAsync(CreateProductRequest request);
    Task<Result<ProductResponse>> GetByIdAsync(Guid id);
    Task<PagedResult<ProductResponse>> GetAllAsync(int page = 1, int pageSize = 20, string? search = null, Guid? categoryId = null, string? sortBy = null, bool sortDesc = false);
    Task<Result<ProductResponse>> UpdateAsync(Guid id, UpdateProductRequest request);
    Task<Result<bool>> DeleteAsync(Guid id);
}
