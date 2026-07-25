using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInvoice.Domain.Entities;

namespace SmartInvoice.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.LogoUrl).HasMaxLength(500);
        builder.Property(c => c.SignatureUrl).HasMaxLength(500);
        builder.Property(c => c.Website).HasMaxLength(200);
        builder.Property(c => c.Phone).HasMaxLength(20);
        builder.Property(c => c.Email).HasMaxLength(200);
        builder.Property(c => c.DefaultCurrency).HasMaxLength(5).HasDefaultValue("INR");
        builder.Property(c => c.TimeZone).HasMaxLength(50).HasDefaultValue("Asia/Kolkata");
        builder.Property(c => c.InvoicePrefix).HasMaxLength(10).HasDefaultValue("INV");

        // Value objects - owned types
        builder.OwnsOne(c => c.Address, a =>
        {
            a.Property(p => p.Street).HasMaxLength(300).HasColumnName("Address_Street");
            a.Property(p => p.City).HasMaxLength(100).HasColumnName("Address_City");
            a.Property(p => p.State).HasMaxLength(100).HasColumnName("Address_State");
            a.Property(p => p.PostalCode).HasMaxLength(10).HasColumnName("Address_PostalCode");
            a.Property(p => p.Country).HasMaxLength(50).HasColumnName("Address_Country");
        });

        builder.OwnsOne(c => c.GstInfo, g =>
        {
            g.Property(p => p.Gstin).HasMaxLength(15).HasColumnName("GstInfo_Gstin");
            g.Property(p => p.Pan).HasMaxLength(10).HasColumnName("GstInfo_Pan");
            g.Property(p => p.StateCode).HasMaxLength(50).HasColumnName("GstInfo_StateCode");
        });

        builder.OwnsOne(c => c.BankDetails, b =>
        {
            b.Property(p => p.BankName).HasMaxLength(100).HasColumnName("Bank_BankName");
            b.Property(p => p.AccountNumber).HasMaxLength(30).HasColumnName("Bank_AccountNumber");
            b.Property(p => p.IfscCode).HasMaxLength(11).HasColumnName("Bank_IfscCode");
            b.Property(p => p.AccountHolderName).HasMaxLength(200).HasColumnName("Bank_AccountHolderName");
            b.Property(p => p.BranchName).HasMaxLength(100).HasColumnName("Bank_BranchName");
            b.Property(p => p.UpiId).HasMaxLength(100).HasColumnName("Bank_UpiId");
        });

        builder.HasIndex(c => c.CompanyId);
    }
}
