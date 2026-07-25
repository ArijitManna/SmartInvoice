using SmartInvoice.Application.Common;
using SmartInvoice.Application.Subscriptions.DTOs;

namespace SmartInvoice.Application.Subscriptions;

public interface ISubscriptionService
{
    Task<Result<SubscriptionResponse>> GetCurrentAsync();
    Task<IReadOnlyList<PlanResponse>> GetPlansAsync();
    Task<Result<SubscriptionResponse>> UpgradeAsync(Guid planId);
    Task<bool> CanCreateInvoiceAsync();
    Task<bool> CanCreateCustomerAsync();
    Task IncrementInvoiceCountAsync();
    Task IncrementCustomerCountAsync();
}
