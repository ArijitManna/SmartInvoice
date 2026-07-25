using SmartInvoice.Application.Common;
using SmartInvoice.Application.Customers.DTOs;

namespace SmartInvoice.Application.Customers;

public interface ICustomerService
{
    Task<Result<CustomerResponse>> CreateAsync(CreateCustomerRequest request);
    Task<Result<CustomerResponse>> GetByIdAsync(Guid id);
    Task<PagedResult<CustomerResponse>> GetAllAsync(int page = 1, int pageSize = 20, string? search = null, string? sortBy = null, bool sortDesc = false);
    Task<Result<CustomerResponse>> UpdateAsync(Guid id, UpdateCustomerRequest request);
    Task<Result<bool>> DeleteAsync(Guid id);
}
