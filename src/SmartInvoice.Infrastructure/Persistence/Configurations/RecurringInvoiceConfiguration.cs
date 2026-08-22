using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInvoice.Domain.Entities;

namespace SmartInvoice.Infrastructure.Persistence.Configurations;

public class RecurringInvoiceConfiguration : IEntityTypeConfiguration<RecurringInvoice>
{
    public void Configure(EntityTypeBuilder<RecurringInvoice> builder)
    {
        builder.ToTable("RecurringInvoices");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Notes).HasMaxLength(2000);
        builder.Property(r => r.TermsAndConditions).HasMaxLength(5000);
        builder.Property(r => r.DiscountPercentage).HasPrecision(5, 2);
        builder.Property(r => r.ItemsJson).HasColumnType("jsonb");
        builder.HasOne(r => r.Customer).WithMany().HasForeignKey(r => r.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(r => r.CompanyId);
        builder.HasIndex(r => r.NextGenerationDate);
    }
}

public class InvoiceTemplateConfiguration : IEntityTypeConfiguration<InvoiceTemplate>
{
    public void Configure(EntityTypeBuilder<InvoiceTemplate> builder)
    {
        builder.ToTable("InvoiceTemplates");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.TemplateKey).IsRequired().HasMaxLength(50);
        builder.Property(t => t.Description).HasMaxLength(250);
        builder.Property(t => t.PreviewImageUrl).HasMaxLength(500);
        builder.HasIndex(t => t.CompanyId);
    }
}
