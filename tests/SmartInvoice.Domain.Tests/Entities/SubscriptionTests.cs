using SmartInvoice.Domain.Entities;
using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Domain.Tests.Entities;

public class SubscriptionTests
{
    private static Subscription CreateFreeSubscription(int invoicesUsed = 0, int customersUsed = 0)
    {
        return new Subscription
        {
            Status = SubscriptionStatus.Active,
            InvoicesCreatedThisMonth = invoicesUsed,
            CustomersCreated = customersUsed,
            Plan = new Plan
            {
                Name = "Free",
                Type = PlanType.Free,
                MaxInvoicesPerMonth = 100,
                MaxCustomers = 100
            }
        };
    }

    [Fact]
    public void CanCreateInvoice_UnderLimit_ReturnsTrue()
    {
        var sub = CreateFreeSubscription(invoicesUsed: 50);

        Assert.True(sub.CanCreateInvoice());
    }

    [Fact]
    public void CanCreateInvoice_AtLimit_ReturnsFalse()
    {
        var sub = CreateFreeSubscription(invoicesUsed: 100);

        Assert.False(sub.CanCreateInvoice());
    }

    [Fact]
    public void CanCreateInvoice_ExpiredSubscription_ReturnsFalse()
    {
        var sub = CreateFreeSubscription();
        sub.Status = SubscriptionStatus.Expired;

        Assert.False(sub.CanCreateInvoice());
    }

    [Fact]
    public void CanCreateCustomer_UnderLimit_ReturnsTrue()
    {
        var sub = CreateFreeSubscription(customersUsed: 50);

        Assert.True(sub.CanCreateCustomer());
    }

    [Fact]
    public void CanCreateCustomer_AtLimit_ReturnsFalse()
    {
        var sub = CreateFreeSubscription(customersUsed: 100);

        Assert.False(sub.CanCreateCustomer());
    }

    [Fact]
    public void ResetMonthlyCounters_ResetsInvoiceCount()
    {
        var sub = CreateFreeSubscription(invoicesUsed: 75);

        sub.ResetMonthlyCounters();

        Assert.Equal(0, sub.InvoicesCreatedThisMonth);
        Assert.NotNull(sub.LastResetDate);
    }

    [Fact]
    public void CanCreateInvoice_UnlimitedPlan_AlwaysReturnsTrue()
    {
        var sub = new Subscription
        {
            Status = SubscriptionStatus.Active,
            InvoicesCreatedThisMonth = 999,
            Plan = new Plan
            {
                Name = "Enterprise",
                Type = PlanType.Enterprise,
                MaxInvoicesPerMonth = 0 // 0 means unlimited
            }
        };

        Assert.True(sub.CanCreateInvoice());
    }
}
