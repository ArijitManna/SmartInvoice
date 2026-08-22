using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInvoice.Domain.Entities;

namespace SmartInvoice.Infrastructure.Persistence.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Name).IsRequired().HasMaxLength(200);
        builder.Property(w => w.Code).HasMaxLength(20);
        builder.Property(w => w.Street).HasMaxLength(300);
        builder.Property(w => w.City).HasMaxLength(100);
        builder.Property(w => w.State).HasMaxLength(100);
        builder.Property(w => w.PostalCode).HasMaxLength(10);
        builder.Property(w => w.Country).HasMaxLength(50);
        builder.Property(w => w.ContactPerson).HasMaxLength(200);
        builder.Property(w => w.Phone).HasMaxLength(20);
        builder.HasIndex(w => w.CompanyId);
        builder.HasIndex(w => new { w.CompanyId, w.Code });
    }
}
