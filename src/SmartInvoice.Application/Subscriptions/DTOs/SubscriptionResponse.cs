using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Application.Subscriptions.DTOs;

public record SubscriptionResponse(
    Guid Id,
    Guid PlanId,
    string PlanName,
    PlanType PlanType,
    SubscriptionStatus Status,
    DateTime StartDate,
    DateTime? EndDate,
    int InvoicesCreatedThisMonth,
    int CustomersCreated,
    int MaxInvoicesPerMonth,
    int MaxCustomers,
    bool CanCreateInvoice,
    bool CanCreateCustomer
);
