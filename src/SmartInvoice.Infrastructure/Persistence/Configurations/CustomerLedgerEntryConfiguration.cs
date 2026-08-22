using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInvoice.Domain.Entities;

namespace SmartInvoice.Infrastructure.Persistence.Configurations;

public class CustomerLedgerEntryConfiguration : IEntityTypeConfiguration<CustomerLedgerEntry>
{
    public void Configure(EntityTypeBuilder<CustomerLedgerEntry> builder)
    {
        builder.ToTable("CustomerLedgerEntries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Debit).HasPrecision(18, 2);
        builder.Property(e => e.Credit).HasPrecision(18, 2);
        builder.Property(e => e.RunningBalance).HasPrecision(18, 2);
        builder.Property(e => e.ReferenceNumber).HasMaxLength(100);
        builder.Property(e => e.Notes).HasMaxLength(500);

        builder.HasOne(e => e.Customer)
            .WithMany(c => c.LedgerEntries)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.CustomerId);
        builder.HasIndex(e => e.CompanyId);
        builder.HasIndex(e => e.Date);
    }
}
