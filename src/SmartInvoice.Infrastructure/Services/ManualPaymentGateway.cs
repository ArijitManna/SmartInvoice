using SmartInvoice.Application.Subscriptions;

namespace SmartInvoice.Infrastructure.Services;

/// <summary>
/// MVP payment gateway that does nothing.
/// Subscriptions are managed manually (admin upgrades plans in DB).
/// Replace with RazorpayPaymentGateway when integrating real payments.
/// </summary>
public class ManualPaymentGateway : IPaymentGateway
{
    public Task<string> CreateSubscriptionAsync(Guid companyId, Guid planId)
    {
        // In MVP, subscriptions are created directly without payment processing
        return Task.FromResult($"MANUAL-{companyId}-{planId}");
    }

    public Task<bool> CancelSubscriptionAsync(string subscriptionId)
    {
        return Task.FromResult(true);
    }

    public Task<bool> HandleWebhookAsync(string payload, string signature)
    {
        // No-op for manual gateway
        return Task.FromResult(true);
    }
}
