using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartInvoice.Domain.Entities;
using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Infrastructure.Persistence.Seed;

public static class SeedData
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        SeedPlans(modelBuilder);
    }

    public static void SeedRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = "00000000-0000-0000-0000-000000000001",
                Name = "SuperAdmin",
                NormalizedName = "SUPERADMIN",
                ConcurrencyStamp = "1"
            },
            new IdentityRole
            {
                Id = "00000000-0000-0000-0000-000000000002",
                Name = "BusinessOwner",
                NormalizedName = "BUSINESSOWNER",
                ConcurrencyStamp = "2"
            },
            new IdentityRole
            {
                Id = "00000000-0000-0000-0000-000000000003",
                Name = "Accountant",
                NormalizedName = "ACCOUNTANT",
                ConcurrencyStamp = "3"
            },
            new IdentityRole
            {
                Id = "00000000-0000-0000-0000-000000000004",
                Name = "Staff",
                NormalizedName = "STAFF",
                ConcurrencyStamp = "4"
            },
            new IdentityRole
            {
                Id = "00000000-0000-0000-0000-000000000005",
                Name = "Viewer",
                NormalizedName = "VIEWER",
                ConcurrencyStamp = "5"
            }
        );
    }

    private static void SeedPlans(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Plan>().HasData(
            new Plan
            {
                Id = new Guid("00000000-0000-0000-0000-000000000001"),
                Name = "Free",
                Type = PlanType.Free,
                MonthlyPrice = 0,
                YearlyPrice = 0,
                MaxInvoicesPerMonth = 100,
                MaxCustomers = 100,
                MaxBusinesses = 1,
                HasRecurringInvoice = false,
                HasAdvancedReports = false,
                HasAiFeatures = false,
                HasWhiteLabel = false,
                HasApiAccess = false,
                IsActive = true
            },
            new Plan
            {
                Id = new Guid("00000000-0000-0000-0000-000000000002"),
                Name = "Starter",
                Type = PlanType.Starter,
                MonthlyPrice = 299,
                YearlyPrice = 2990,
                MaxInvoicesPerMonth = 0, // Unlimited
                MaxCustomers = 0,        // Unlimited
                MaxBusinesses = 1,
                HasRecurringInvoice = false,
                HasAdvancedReports = false,
                HasAiFeatures = false,
                HasWhiteLabel = false,
                HasApiAccess = false,
                IsActive = true
            },
            new Plan
            {
                Id = new Guid("00000000-0000-0000-0000-000000000003"),
                Name = "Professional",
                Type = PlanType.Professional,
                MonthlyPrice = 699,
                YearlyPrice = 6990,
                MaxInvoicesPerMonth = 0,
                MaxCustomers = 0,
                MaxBusinesses = 3,
                HasRecurringInvoice = true,
                HasAdvancedReports = true,
                HasAiFeatures = true,
                HasWhiteLabel = false,
                HasApiAccess = false,
                IsActive = true
            },
            new Plan
            {
                Id = new Guid("00000000-0000-0000-0000-000000000004"),
                Name = "Enterprise",
                Type = PlanType.Enterprise,
                MonthlyPrice = 0, // Custom pricing
                YearlyPrice = 0,
                MaxInvoicesPerMonth = 0,
                MaxCustomers = 0,
                MaxBusinesses = 0, // Unlimited
                HasRecurringInvoice = true,
                HasAdvancedReports = true,
                HasAiFeatures = true,
                HasWhiteLabel = true,
                HasApiAccess = true,
                IsActive = true
            }
        );
    }
}
