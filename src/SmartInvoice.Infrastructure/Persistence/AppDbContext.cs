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

        // e.CompanyId == CurrentCompanyId.Value — but we compare as Guid? to avoid .Value on null
        var entityCompanyId = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.CompanyId));
        var entityCompanyIdNullable = System.Linq.Expressions.Expression.Convert(entityCompanyId, typeof(Guid?));

        var companyIdEquals = System.Linq.Expressions.Expression.Equal(entityCompanyIdNullable, currentCompanyIdProp);

        // CurrentCompanyId == null (bypass tenant filter when no company context)
        var companyIdIsNull = System.Linq.Expressions.Expression.Equal(
            currentCompanyIdProp,
            System.Linq.Expressions.Expression.Constant(null, typeof(Guid?)));

        var tenantFilter = System.Linq.Expressions.Expression.OrElse(companyIdIsNull, companyIdEquals);
        var combined = System.Linq.Expressions.Expression.AndAlso(notDeleted, tenantFilter);

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
