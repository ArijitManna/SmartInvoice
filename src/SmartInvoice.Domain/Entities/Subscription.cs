using SmartInvoice.Domain.Common;
using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Domain.Entities;

public class Subscription : BaseEntity
{
    public Guid PlanId { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public int InvoicesCreatedThisMonth { get; set; }
    public int CustomersCreated { get; set; }
    public DateTime? LastResetDate { get; set; }

    // Navigation
    public Plan Plan { get; set; } = null!;

    /// <summary>
    /// Checks whether the subscription can create another invoice based on plan limits.
    /// </summary>
    public bool CanCreateInvoice()
    {
        if (Status != SubscriptionStatus.Active && Status != SubscriptionStatus.Trial)
            return false;

        return Plan.MaxInvoicesPerMonth <= 0 || InvoicesCreatedThisMonth < Plan.MaxInvoicesPerMonth;
    }

    /// <summary>
    /// Checks whether the subscription can add another customer based on plan limits.
    /// </summary>
    public bool CanCreateCustomer()
    {
        if (Status != SubscriptionStatus.Active && Status != SubscriptionStatus.Trial)
            return false;

        return Plan.MaxCustomers <= 0 || CustomersCreated < Plan.MaxCustomers;
    }

    /// <summary>
    /// Resets the monthly invoice counter. Should be called at the start of each billing period.
    /// </summary>
    public void ResetMonthlyCounters()
    {
        InvoicesCreatedThisMonth = 0;
        LastResetDate = DateTime.UtcNow;
    }
}
