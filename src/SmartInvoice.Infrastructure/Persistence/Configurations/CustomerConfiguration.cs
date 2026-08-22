using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInvoice.Domain.Entities;

namespace SmartInvoice.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Email).HasMaxLength(200);
        builder.Property(c => c.Phone).HasMaxLength(20);
        builder.Property(c => c.ContactPerson).HasMaxLength(200);
        builder.Property(c => c.Notes).HasMaxLength(2000);

        builder.Property(c => c.CreditLimit).HasPrecision(18, 2);
        builder.Property(c => c.OutstandingBalance).HasPrecision(18, 2);
        builder.Property(c => c.OpeningBalance).HasPrecision(18, 2);

        builder.OwnsOne(c => c.GstInfo, g =>
        {
            g.Property(p => p.Gstin).HasMaxLength(15).HasColumnName("GstInfo_Gstin");
            g.Property(p => p.Pan).HasMaxLength(10).HasColumnName("GstInfo_Pan");
            g.Property(p => p.StateCode).HasMaxLength(50).HasColumnName("GstInfo_StateCode");
        });

        builder.OwnsOne(c => c.BillingAddress, a =>
        {
            a.Property(p => p.Street).HasMaxLength(300).HasColumnName("Billing_Street");
            a.Property(p => p.City).HasMaxLength(100).HasColumnName("Billing_City");
            a.Property(p => p.State).HasMaxLength(100).HasColumnName("Billing_State");
            a.Property(p => p.PostalCode).HasMaxLength(10).HasColumnName("Billing_PostalCode");
            a.Property(p => p.Country).HasMaxLength(50).HasColumnName("Billing_Country");
        });

        builder.OwnsOne(c => c.ShippingAddress, a =>
        {
            a.Property(p => p.Street).HasMaxLength(300).HasColumnName("Shipping_Street");
            a.Property(p => p.City).HasMaxLength(100).HasColumnName("Shipping_City");
            a.Property(p => p.State).HasMaxLength(100).HasColumnName("Shipping_State");
            a.Property(p => p.PostalCode).HasMaxLength(10).HasColumnName("Shipping_PostalCode");
            a.Property(p => p.Country).HasMaxLength(50).HasColumnName("Shipping_Country");
        });

        builder.HasIndex(c => c.CompanyId);
        builder.HasIndex(c => new { c.CompanyId, c.Email });
    }
}
