using Microsoft.EntityFrameworkCore;
using SmartInvoice.Application.Common;
using SmartInvoice.Application.Subscriptions;
using SmartInvoice.Application.Subscriptions.DTOs;
using SmartInvoice.Domain.Entities;
using SmartInvoice.Domain.Enums;
using SmartInvoice.Infrastructure.Persistence;

namespace SmartInvoice.Infrastructure.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly AppDbContext _context;
    private readonly ICurrentCompanyService _companyService;

    public SubscriptionService(AppDbContext context, ICurrentCompanyService companyService)
    {
        _context = context;
        _companyService = companyService;
    }

    public async Task<Result<SubscriptionResponse>> GetCurrentAsync()
    {
        var subscription = await GetOrCreateSubscriptionAsync();
        if (subscription is null)
        {
            return Result<SubscriptionResponse>.Failure("No company context.");
        }

        return Result<SubscriptionResponse>.Success(MapToResponse(subscription));
    }

    public async Task<IReadOnlyList<PlanResponse>> GetPlansAsync()
    {
        var plans = await _context.Plans
            .IgnoreQueryFilters()
            .Where(p => p.IsActive)
            .OrderBy(p => p.MonthlyPrice)
            .ToListAsync();

        return plans.Select(p => new PlanResponse(
            p.Id, p.Name, p.Type, p.MonthlyPrice, p.YearlyPrice,
            p.MaxInvoicesPerMonth, p.MaxCustomers, p.MaxBusinesses,
            p.HasRecurringInvoice, p.HasAdvancedReports, p.HasAiFeatures,
            p.HasWhiteLabel, p.HasApiAccess
        )).ToList().AsReadOnly();
    }

    public async Task<Result<SubscriptionResponse>> UpgradeAsync(Guid planId)
    {
        var companyId = _companyService.CompanyId;
        if (!companyId.HasValue)
        {
            return Result<SubscriptionResponse>.Failure("No company context.");
        }

        var plan = await _context.Plans
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == planId && p.IsActive);

        if (plan is null)
        {
            return Result<SubscriptionResponse>.Failure("Plan not found.");
        }

        var subscription = await GetOrCreateSubscriptionAsync();
        if (subscription is null)
        {
            return Result<SubscriptionResponse>.Failure("No company context.");
        }

        subscription.PlanId = planId;
        subscription.Plan = plan;
        subscription.Status = SubscriptionStatus.Active;
        subscription.StartDate = DateTime.UtcNow;
        subscription.EndDate = DateTime.UtcNow.AddMonths(1);

        await _context.SaveChangesAsync();

        return Result<SubscriptionResponse>.Success(MapToResponse(subscription));
    }

    public async Task<bool> CanCreateInvoiceAsync()
    {
        var subscription = await GetOrCreateSubscriptionAsync();
        return subscription?.CanCreateInvoice() ?? false;
    }

    public async Task<bool> CanCreateCustomerAsync()
    {
        var subscription = await GetOrCreateSubscriptionAsync();
        return subscription?.CanCreateCustomer() ?? false;
    }

    public async Task IncrementInvoiceCountAsync()
    {
        var subscription = await GetOrCreateSubscriptionAsync();
        if (subscription is not null)
        {
            subscription.InvoicesCreatedThisMonth++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task IncrementCustomerCountAsync()
    {
        var subscription = await GetOrCreateSubscriptionAsync();
        if (subscription is not null)
        {
            subscription.CustomersCreated++;
            await _context.SaveChangesAsync();
        }
    }

    private async Task<Subscription?> GetOrCreateSubscriptionAsync()
    {
        var companyId = _companyService.CompanyId;
        if (!companyId.HasValue)
        {
            return null;
        }

        var subscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.CompanyId == companyId.Value);

        if (subscription is not null)
        {
            // Reset monthly counter if new month
            if (subscription.LastResetDate.HasValue &&
                subscription.LastResetDate.Value.Month != DateTime.UtcNow.Month)
            {
                subscription.ResetMonthlyCounters();
                await _context.SaveChangesAsync();
            }

            return subscription;
        }

        // Auto-create Free subscription for new companies
        var freePlan = await _context.Plans
            .IgnoreQueryFilters()
            .FirstAsync(p => p.Type == PlanType.Free && p.IsActive);

        subscription = new Subscription
        {
            CompanyId = companyId.Value,
            PlanId = freePlan.Id,
            Plan = freePlan,
            Status = SubscriptionStatus.Active,
            StartDate = DateTime.UtcNow
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        return subscription;
    }

    private static SubscriptionResponse MapToResponse(Subscription sub)
    {
        return new SubscriptionResponse(
            Id: sub.Id,
            PlanId: sub.PlanId,
            PlanName: sub.Plan.Name,
            PlanType: sub.Plan.Type,
            Status: sub.Status,
            StartDate: sub.StartDate,
            EndDate: sub.EndDate,
            InvoicesCreatedThisMonth: sub.InvoicesCreatedThisMonth,
            CustomersCreated: sub.CustomersCreated,
            MaxInvoicesPerMonth: sub.Plan.MaxInvoicesPerMonth,
            MaxCustomers: sub.Plan.MaxCustomers,
            CanCreateInvoice: sub.CanCreateInvoice(),
            CanCreateCustomer: sub.CanCreateCustomer()
        );
    }
}
