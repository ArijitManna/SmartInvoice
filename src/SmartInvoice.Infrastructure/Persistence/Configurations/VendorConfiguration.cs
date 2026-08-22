using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInvoice.Domain.Entities;

namespace SmartInvoice.Infrastructure.Persistence.Configurations;

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("Vendors");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Name).IsRequired().HasMaxLength(200);
        builder.Property(v => v.Email).HasMaxLength(200);
        builder.Property(v => v.Phone).HasMaxLength(20);
        builder.Property(v => v.ContactPerson).HasMaxLength(200);
        builder.Property(v => v.Notes).HasMaxLength(2000);
        builder.Property(v => v.OutstandingBalance).HasPrecision(18, 2);
        builder.Property(v => v.OpeningBalance).HasPrecision(18, 2);

        builder.OwnsOne(v => v.GstInfo, g => {
            g.Property(p => p.Gstin).HasMaxLength(15).HasColumnName("GstInfo_Gstin");
            g.Property(p => p.Pan).HasMaxLength(10).HasColumnName("GstInfo_Pan");
            g.Property(p => p.StateCode).HasMaxLength(50).HasColumnName("GstInfo_StateCode");
        });
        builder.OwnsOne(v => v.Address, a => {
            a.Property(p => p.Street).HasMaxLength(300).HasColumnName("Address_Street");
            a.Property(p => p.City).HasMaxLength(100).HasColumnName("Address_City");
            a.Property(p => p.State).HasMaxLength(100).HasColumnName("Address_State");
            a.Property(p => p.PostalCode).HasMaxLength(10).HasColumnName("Address_PostalCode");
            a.Property(p => p.Country).HasMaxLength(50).HasColumnName("Address_Country");
        });
        builder.OwnsOne(v => v.BankDetails, b => {
            b.Property(p => p.BankName).HasMaxLength(100).HasColumnName("Bank_BankName");
            b.Property(p => p.AccountNumber).HasMaxLength(30).HasColumnName("Bank_AccountNumber");
            b.Property(p => p.IfscCode).HasMaxLength(11).HasColumnName("Bank_IfscCode");
            b.Property(p => p.AccountHolderName).HasMaxLength(200).HasColumnName("Bank_AccountHolderName");
            b.Property(p => p.BranchName).HasMaxLength(100).HasColumnName("Bank_BranchName");
            b.Property(p => p.UpiId).HasMaxLength(100).HasColumnName("Bank_UpiId");
        });

        builder.HasIndex(v => v.CompanyId);
    }
}
