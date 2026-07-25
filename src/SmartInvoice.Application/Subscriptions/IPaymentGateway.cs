namespace SmartInvoice.Application.Subscriptions;

/// <summary>
/// Abstraction for payment gateway integration.
/// MVP uses ManualPaymentGateway; Razorpay implementation plugs in later.
/// </summary>
public interface IPaymentGateway
{
    Task<string> CreateSubscriptionAsync(Guid companyId, Guid planId);
    Task<bool> CancelSubscriptionAsync(string subscriptionId);
    Task<bool> HandleWebhookAsync(string payload, string signature);
}
