using SmartInvoice.Application.Common;
using SmartInvoice.Application.Invoices.DTOs;
using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Application.Invoices;

public interface IInvoiceService
{
    Task<Result<InvoiceResponse>> CreateAsync(CreateInvoiceRequest request);
    Task<Result<InvoiceResponse>> GetByIdAsync(Guid id);
    Task<PagedResult<InvoiceListResponse>> GetAllAsync(int page = 1, int pageSize = 20, InvoiceStatus? status = null, Guid? customerId = null, DateTime? from = null, DateTime? to = null, string? sortBy = null, bool sortDesc = false);
    Task<Result<InvoiceResponse>> UpdateAsync(Guid id, UpdateInvoiceRequest request);
    Task<Result<InvoiceResponse>> DuplicateAsync(Guid id);
    Task<Result<bool>> DeleteAsync(Guid id);
}
