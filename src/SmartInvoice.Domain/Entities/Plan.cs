using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Domain.Entities;

/// <summary>
/// Represents a subscription plan. Not tenant-scoped (global data).
/// </summary>
public class Plan
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public PlanType Type { get; set; }
    public decimal MonthlyPrice { get; set; }
    public decimal? YearlyPrice { get; set; }
    public int MaxInvoicesPerMonth { get; set; }
    public int MaxCustomers { get; set; }
    public int MaxBusinesses { get; set; }
    public bool HasRecurringInvoice { get; set; }
    public bool HasAdvancedReports { get; set; }
    public bool HasAiFeatures { get; set; }
    public bool HasWhiteLabel { get; set; }
    public bool HasApiAccess { get; set; }
    public bool IsActive { get; set; } = true;
}
