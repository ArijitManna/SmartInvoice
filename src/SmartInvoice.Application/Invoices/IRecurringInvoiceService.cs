using SmartInvoice.Application.Common;
using SmartInvoice.Application.Invoices.DTOs;

namespace SmartInvoice.Application.Invoices;

public interface IRecurringInvoiceService
{
    Task<Result<RecurringInvoiceResponse>> CreateAsync(CreateRecurringInvoiceRequest request);
    Task<IReadOnlyList<RecurringInvoiceResponse>> GetAllAsync();
    Task<Result<bool>> DeactivateAsync(Guid id);
    Task<int> ProcessDueRecurringInvoicesAsync();
}
