using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartInvoice.Domain.Common;
using SmartInvoice.Domain.Entities;
using SmartInvoice.Infrastructure.Identity;
using SmartInvoice.Infrastructure.Persistence.Seed;

namespace SmartInvoice.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly ICurrentCompanyService? _companyService;

    public Guid? CurrentCompanyId => _companyService?.CompanyId;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentCompanyService? companyService)
        : base(options)
    {
        _companyService = companyService;
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<CustomerLedgerEntry> CustomerLedgerEntries => Set<CustomerLedgerEntry>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<StockEntry> StockEntries => Set<StockEntry>();
    public DbSet<Batch> Batches => Set<Batch>();
    public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();
    public DbSet<StockTransferItem> StockTransferItems => Set<StockTransferItem>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<PurchaseBill> PurchaseBills => Set<PurchaseBill>();
    public DbSet<PurchaseBillItem> PurchaseBillItems => Set<PurchaseBillItem>();
    public DbSet<PurchaseReturn> PurchaseReturns => Set<PurchaseReturn>();
    public DbSet<VendorLedgerEntry> VendorLedgerEntries => Set<VendorLedgerEntry>();
    public DbSet<VendorPayment> VendorPayments => Set<VendorPayment>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<RecurringInvoice> RecurringInvoices => Set<RecurringInvoice>();
    public DbSet<InvoiceTemplate> InvoiceTemplates => Set<InvoiceTemplate>();
    public DbSet<ImportJob> ImportJobs => Set<ImportJob>();
    public DbSet<ErrorLog> ErrorLogs => Set<ErrorLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Seed data
        SeedData.Apply(modelBuilder);

        // Configure ApplicationUser
        modelBuilder.Entity<ApplicationUser>(b =>
        {
            b.Property(u => u.FirstName).HasMaxLength(100);
            b.Property(u => u.LastName).HasMaxLength(100);
            b.Property(u => u.RefreshToken).HasMaxLength(500);
            b.HasIndex(u => u.CompanyId);
        });

        // Seed roles
        SeedData.SeedRoles(modelBuilder);

        // Global query filters for multi-tenancy and soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (clrType.IsAssignableTo(typeof(BaseEntity)))
            {
                modelBuilder.Entity(clrType).HasQueryFilter(
                    BuildTenantAndSoftDeleteFilter(clrType));
            }
        }
    }

    private System.Linq.Expressions.LambdaExpression BuildTenantAndSoftDeleteFilter(Type entityType)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");

        var isDeletedProp = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
        var notDeleted = System.Linq.Expressions.Expression.Equal(
            isDeletedProp,
            System.Linq.Expressions.Expression.Constant(false));

        var dbContextExpr = System.Linq.Expressions.Expression.Constant(this);
        var currentCompanyIdProp = System.Linq.Expressions.Expression.Property(dbContextExpr, nameof(CurrentCompanyId));

        // e.CompanyId == CurrentCompanyId (strict filtering - no bypass for null)
        var entityCompanyId = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.CompanyId));
        var entityCompanyIdNullable = System.Linq.Expressions.Expression.Convert(entityCompanyId, typeof(Guid?));

        var companyIdEquals = System.Linq.Expressions.Expression.Equal(entityCompanyIdNullable, currentCompanyIdProp);

        // SECURITY FIX: Removed the null bypass condition
        // BEFORE: (CurrentCompanyId == null) OR (e.CompanyId == CurrentCompanyId)  — allowed data leak
        // AFTER:  (e.CompanyId == CurrentCompanyId)                               — strict filtering
        // When CurrentCompanyId is null, NO records are visible (secure default)
        var combined = System.Linq.Expressions.Expression.AndAlso(notDeleted, companyIdEquals);

        return System.Linq.Expressions.Expression.Lambda(combined, parameter);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var companyId = CurrentCompanyId;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    if (companyId.HasValue && entry.Entity.CompanyId == Guid.Empty)
                    {
                        entry.Entity.CompanyId = companyId.Value;
                    }
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
