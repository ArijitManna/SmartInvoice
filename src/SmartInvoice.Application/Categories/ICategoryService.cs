using SmartInvoice.Application.Categories.DTOs;
using SmartInvoice.Application.Common;

namespace SmartInvoice.Application.Categories;

public interface ICategoryService
{
    Task<Result<CategoryResponse>> CreateAsync(CreateCategoryRequest request);
    Task<IReadOnlyList<CategoryResponse>> GetAllAsync();
    Task<Result<CategoryResponse>> UpdateAsync(Guid id, CreateCategoryRequest request);
    Task<Result<bool>> DeleteAsync(Guid id);
}
