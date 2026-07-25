using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInvoice.Domain.Entities;

namespace SmartInvoice.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.Sku).HasMaxLength(50);
        builder.Property(p => p.HsnSacCode).HasMaxLength(20);
        builder.Property(p => p.Unit).HasMaxLength(20).HasDefaultValue("Nos");
        builder.Property(p => p.Price).HasPrecision(18, 2);
        builder.Property(p => p.TaxRate).HasPrecision(5, 2);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(p => p.CompanyId);
        builder.HasIndex(p => new { p.CompanyId, p.Sku });
    }
}
