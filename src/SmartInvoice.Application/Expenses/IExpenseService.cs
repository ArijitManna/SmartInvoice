using SmartInvoice.Application.Common;
using SmartInvoice.Application.Expenses.DTOs;

namespace SmartInvoice.Application.Expenses;

public interface IExpenseService
{
    Task<Result<ExpenseResponse>> CreateAsync(CreateExpenseRequest request);
    Task<PagedResult<ExpenseResponse>> GetAllAsync(int page = 1, int pageSize = 20, Guid? categoryId = null, DateTime? from = null, DateTime? to = null);
    Task<Result<ExpenseResponse>> UpdateAsync(Guid id, CreateExpenseRequest request);
    Task<Result<bool>> DeleteAsync(Guid id);
    Task<ExpenseSummaryResponse> GetSummaryAsync(DateTime? from = null, DateTime? to = null);

    // Categories
    Task<Result<ExpenseCategoryResponse>> CreateCategoryAsync(ExpenseCategoryRequest request);
    Task<IReadOnlyList<ExpenseCategoryResponse>> GetCategoriesAsync();
    Task<Result<bool>> DeleteCategoryAsync(Guid id);
}
