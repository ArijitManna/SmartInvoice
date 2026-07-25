using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInvoice.Domain.Entities;

namespace SmartInvoice.Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.SubTotal).HasPrecision(18, 2);
        builder.Property(i => i.DiscountAmount).HasPrecision(18, 2);
        builder.Property(i => i.DiscountPercentage).HasPrecision(5, 2);
        builder.Property(i => i.TaxAmount).HasPrecision(18, 2);
        builder.Property(i => i.CgstAmount).HasPrecision(18, 2);
        builder.Property(i => i.SgstAmount).HasPrecision(18, 2);
        builder.Property(i => i.IgstAmount).HasPrecision(18, 2);
        builder.Property(i => i.TotalAmount).HasPrecision(18, 2);
        builder.Property(i => i.AmountPaid).HasPrecision(18, 2);
        builder.Property(i => i.BalanceDue).HasPrecision(18, 2);
        builder.Property(i => i.Currency).HasMaxLength(5).HasDefaultValue("INR");

        builder.Property(i => i.Notes).HasMaxLength(2000);
        builder.Property(i => i.TermsAndConditions).HasMaxLength(5000);
        builder.Property(i => i.ReferenceNumber).HasMaxLength(100);

        builder.HasOne(i => i.Customer)
            .WithMany(c => c.Invoices)
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.CompanyId);
        builder.HasIndex(i => new { i.CompanyId, i.InvoiceNumber }).IsUnique();
        builder.HasIndex(i => new { i.CompanyId, i.Status });
        builder.HasIndex(i => i.DueDate);
    }
}
