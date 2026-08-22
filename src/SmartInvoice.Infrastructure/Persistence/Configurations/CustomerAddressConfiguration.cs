using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInvoice.Domain.Entities;

namespace SmartInvoice.Infrastructure.Persistence.Configurations;

public class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.ToTable("CustomerAddresses");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Label).HasMaxLength(100);
        builder.Property(a => a.Street).HasMaxLength(300);
        builder.Property(a => a.City).HasMaxLength(100);
        builder.Property(a => a.State).HasMaxLength(100);
        builder.Property(a => a.PostalCode).HasMaxLength(10);
        builder.Property(a => a.Country).HasMaxLength(50).HasDefaultValue("India");

        builder.HasOne(a => a.Customer)
            .WithMany(c => c.Addresses)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.CustomerId);
        builder.HasIndex(a => a.CompanyId);
    }
}
