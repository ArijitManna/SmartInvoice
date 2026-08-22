using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInvoice.Domain.Entities;

namespace SmartInvoice.Infrastructure.Persistence.Configurations;

public class ExpenseCategoryConfiguration : IEntityTypeConfiguration<ExpenseCategory>
{
    public void Configure(EntityTypeBuilder<ExpenseCategory> builder)
    {
        builder.ToTable("ExpenseCategories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.HasOne(c => c.Parent).WithMany(c => c.Children).HasForeignKey(c => c.ParentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(c => c.CompanyId);
    }
}

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.Property(e => e.TaxAmount).HasPrecision(18, 2);
        builder.Property(e => e.TotalAmount).HasPrecision(18, 2);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(500);
        builder.Property(e => e.ReferenceNumber).HasMaxLength(100);
        builder.Property(e => e.ReceiptUrl).HasMaxLength(500);
        builder.HasOne(e => e.Category).WithMany(c => c.Expenses).HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Vendor).WithMany().HasForeignKey(e => e.VendorId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(e => e.CompanyId);
        builder.HasIndex(e => e.CategoryId);
        builder.HasIndex(e => e.Date);
    }
}
