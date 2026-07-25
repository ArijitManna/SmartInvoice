using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Application.Subscriptions.DTOs;

public record PlanResponse(
    Guid Id,
    string Name,
    PlanType Type,
    decimal MonthlyPrice,
    decimal? YearlyPrice,
    int MaxInvoicesPerMonth,
    int MaxCustomers,
    int MaxBusinesses,
    bool HasRecurringInvoice,
    bool HasAdvancedReports,
    bool HasAiFeatures,
    bool HasWhiteLabel,
    bool HasApiAccess
);
