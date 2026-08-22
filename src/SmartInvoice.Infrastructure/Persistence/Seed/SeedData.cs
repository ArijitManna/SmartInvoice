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
        SeedPermissions(modelBuilder);
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

    private static void SeedPermissions(ModelBuilder modelBuilder)
    {
        // Define all permissions
        var permissions = new List<Permission>
        {
            // Invoice
            new() { Id = new Guid("10000000-0000-0000-0000-000000000001"), Name = "Invoice.Create", Module = "Invoice", Action = "Create", Description = "Create invoices" },
            new() { Id = new Guid("10000000-0000-0000-0000-000000000002"), Name = "Invoice.Edit", Module = "Invoice", Action = "Edit", Description = "Edit draft invoices" },
            new() { Id = new Guid("10000000-0000-0000-0000-000000000003"), Name = "Invoice.Delete", Module = "Invoice", Action = "Delete", Description = "Delete invoices" },
            new() { Id = new Guid("10000000-0000-0000-0000-000000000004"), Name = "Invoice.View", Module = "Invoice", Action = "View", Description = "View invoices" },
            new() { Id = new Guid("10000000-0000-0000-0000-000000000005"), Name = "Invoice.Send", Module = "Invoice", Action = "Send", Description = "Send invoices via email" },
            new() { Id = new Guid("10000000-0000-0000-0000-000000000006"), Name = "Invoice.RecordPayment", Module = "Invoice", Action = "RecordPayment", Description = "Record payments against invoices" },
            // Customer
            new() { Id = new Guid("10000000-0000-0000-0000-000000000010"), Name = "Customer.Create", Module = "Customer", Action = "Create", Description = "Create customers" },
            new() { Id = new Guid("10000000-0000-0000-0000-000000000011"), Name = "Customer.Edit", Module = "Customer", Action = "Edit", Description = "Edit customers" },
            new() { Id = new Guid("10000000-0000-0000-0000-000000000012"), Name = "Customer.Delete", Module = "Customer", Action = "Delete", Description = "Delete customers" },
            new() { Id = new Guid("10000000-0000-0000-0000-000000000013"), Name = "Customer.View", Module = "Customer", Action = "View", Description = "View customers" },
            // Product
            new() { Id = new Guid("10000000-0000-0000-0000-000000000020"), Name = "Product.Create", Module = "Product", Action = "Create", Description = "Create products" },
            new() { Id = new Guid("10000000-0000-0000-0000-000000000021"), Name = "Product.Edit", Module = "Product", Action = "Edit", Description = "Edit products" },
            new() { Id = new Guid("10000000-0000-0000-0000-000000000022"), Name = "Product.Delete", Module = "Product", Action = "Delete", Description = "Delete products" },
            new() { Id = new Guid("10000000-0000-0000-0000-000000000023"), Name = "Product.View", Module = "Product", Action = "View", Description = "View products" },
            // Report
            new() { Id = new Guid("10000000-0000-0000-0000-000000000030"), Name = "Report.View", Module = "Report", Action = "View", Description = "View reports" },
            new() { Id = new Guid("10000000-0000-0000-0000-000000000031"), Name = "Report.Export", Module = "Report", Action = "Export", Description = "Export reports" },
            // Settings
            new() { Id = new Guid("10000000-0000-0000-0000-000000000040"), Name = "Settings.Manage", Module = "Settings", Action = "Manage", Description = "Manage company settings" },
            // Users
            new() { Id = new Guid("10000000-0000-0000-0000-000000000050"), Name = "User.Manage", Module = "User", Action = "Manage", Description = "Manage users and roles" },
            new() { Id = new Guid("10000000-0000-0000-0000-000000000051"), Name = "User.ViewAll", Module = "User", Action = "ViewAll", Description = "View all users" },
            // Dashboard
            new() { Id = new Guid("10000000-0000-0000-0000-000000000060"), Name = "Dashboard.View", Module = "Dashboard", Action = "View", Description = "View dashboard" },
            // Expense
            new() { Id = new Guid("10000000-0000-0000-0000-000000000070"), Name = "Expense.Create", Module = "Expense", Action = "Create", Description = "Create expenses" },
            new() { Id = new Guid("10000000-0000-0000-0000-000000000071"), Name = "Expense.Edit", Module = "Expense", Action = "Edit", Description = "Edit expenses" },
            new() { Id = new Guid("10000000-0000-0000-0000-000000000072"), Name = "Expense.Delete", Module = "Expense", Action = "Delete", Description = "Delete expenses" },
            new() { Id = new Guid("10000000-0000-0000-0000-000000000073"), Name = "Expense.View", Module = "Expense", Action = "View", Description = "View expenses" },
            // Inventory
            new() { Id = new Guid("10000000-0000-0000-0000-000000000080"), Name = "Inventory.Manage", Module = "Inventory", Action = "Manage", Description = "Manage inventory and stock" },
            new() { Id = new Guid("10000000-0000-0000-0000-000000000081"), Name = "Inventory.View", Module = "Inventory", Action = "View", Description = "View inventory" },
            // Purchase
            new() { Id = new Guid("10000000-0000-0000-0000-000000000090"), Name = "Purchase.Create", Module = "Purchase", Action = "Create", Description = "Create purchase orders/bills" },
            new() { Id = new Guid("10000000-0000-0000-0000-000000000091"), Name = "Purchase.Edit", Module = "Purchase", Action = "Edit", Description = "Edit purchase orders/bills" },
            new() { Id = new Guid("10000000-0000-0000-0000-000000000092"), Name = "Purchase.View", Module = "Purchase", Action = "View", Description = "View purchases" },
            // Import/Export
            new() { Id = new Guid("10000000-0000-0000-0000-0000000000a0"), Name = "Data.Import", Module = "Data", Action = "Import", Description = "Import data from files" },
            new() { Id = new Guid("10000000-0000-0000-0000-0000000000a1"), Name = "Data.Export", Module = "Data", Action = "Export", Description = "Export data to files" },
        };

        modelBuilder.Entity<Permission>().HasData(permissions);

        // Role IDs (from SeedRoles)
        const string adminRoleId = "00000000-0000-0000-0000-000000000001";       // SuperAdmin
        const string ownerRoleId = "00000000-0000-0000-0000-000000000002";       // BusinessOwner
        const string accountantRoleId = "00000000-0000-0000-0000-000000000003";  // Accountant
        const string staffRoleId = "00000000-0000-0000-0000-000000000004";       // Staff
        const string viewerRoleId = "00000000-0000-0000-0000-000000000005";      // Viewer

        var rolePermissions = new List<RolePermission>();
        int counter = 1;

        // SuperAdmin & BusinessOwner get ALL permissions
        foreach (var perm in permissions)
        {
            rolePermissions.Add(new RolePermission { Id = new Guid($"20000000-0000-0000-0000-{counter:D12}"), RoleId = adminRoleId, PermissionId = perm.Id });
            counter++;
            rolePermissions.Add(new RolePermission { Id = new Guid($"20000000-0000-0000-0000-{counter:D12}"), RoleId = ownerRoleId, PermissionId = perm.Id });
            counter++;
        }

        // Accountant: Invoice (all), Customer (all), Product (view), Report, Expense, Dashboard, Purchase, Data.Export
        var accountantPerms = new[] {
            "10000000-0000-0000-0000-000000000001", "10000000-0000-0000-0000-000000000002", "10000000-0000-0000-0000-000000000003",
            "10000000-0000-0000-0000-000000000004", "10000000-0000-0000-0000-000000000005", "10000000-0000-0000-0000-000000000006",
            "10000000-0000-0000-0000-000000000010", "10000000-0000-0000-0000-000000000011", "10000000-0000-0000-0000-000000000012",
            "10000000-0000-0000-0000-000000000013",
            "10000000-0000-0000-0000-000000000023", // Product.View
            "10000000-0000-0000-0000-000000000030", "10000000-0000-0000-0000-000000000031", // Reports
            "10000000-0000-0000-0000-000000000060", // Dashboard
            "10000000-0000-0000-0000-000000000070", "10000000-0000-0000-0000-000000000071", "10000000-0000-0000-0000-000000000072", "10000000-0000-0000-0000-000000000073", // Expense
            "10000000-0000-0000-0000-000000000090", "10000000-0000-0000-0000-000000000091", "10000000-0000-0000-0000-000000000092", // Purchase
            "10000000-0000-0000-0000-0000000000a1", // Data.Export
        };
        foreach (var permId in accountantPerms)
        {
            rolePermissions.Add(new RolePermission { Id = new Guid($"20000000-0000-0000-0000-{counter:D12}"), RoleId = accountantRoleId, PermissionId = new Guid(permId) });
            counter++;
        }

        // Staff: Invoice.Create, Invoice.View, Customer.Create, Customer.View, Product.View, Dashboard
        var staffPerms = new[] {
            "10000000-0000-0000-0000-000000000001", "10000000-0000-0000-0000-000000000004", // Invoice Create/View
            "10000000-0000-0000-0000-000000000010", "10000000-0000-0000-0000-000000000013", // Customer Create/View
            "10000000-0000-0000-0000-000000000023", // Product.View
            "10000000-0000-0000-0000-000000000060", // Dashboard
        };
        foreach (var permId in staffPerms)
        {
            rolePermissions.Add(new RolePermission { Id = new Guid($"20000000-0000-0000-0000-{counter:D12}"), RoleId = staffRoleId, PermissionId = new Guid(permId) });
            counter++;
        }

        // Viewer: only View permissions + Dashboard
        var viewerPerms = new[] {
            "10000000-0000-0000-0000-000000000004", // Invoice.View
            "10000000-0000-0000-0000-000000000013", // Customer.View
            "10000000-0000-0000-0000-000000000023", // Product.View
            "10000000-0000-0000-0000-000000000030", // Report.View
            "10000000-0000-0000-0000-000000000060", // Dashboard
            "10000000-0000-0000-0000-000000000073", // Expense.View
            "10000000-0000-0000-0000-000000000081", // Inventory.View
            "10000000-0000-0000-0000-000000000092", // Purchase.View
        };
        foreach (var permId in viewerPerms)
        {
            rolePermissions.Add(new RolePermission { Id = new Guid($"20000000-0000-0000-0000-{counter:D12}"), RoleId = viewerRoleId, PermissionId = new Guid(permId) });
            counter++;
        }

        modelBuilder.Entity<RolePermission>().HasData(rolePermissions);
    }
}
