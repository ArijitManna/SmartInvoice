using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInvoice.Domain.Entities;

namespace SmartInvoice.Infrastructure.Persistence.Configurations;

public class BatchConfiguration : IEntityTypeConfiguration<Batch>
{
    public void Configure(EntityTypeBuilder<Batch> builder)
    {
        builder.ToTable("Batches");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.BatchNumber).IsRequired().HasMaxLength(100);
        builder.Property(b => b.Quantity).HasPrecision(18, 4);
        builder.Property(b => b.CostPrice).HasPrecision(18, 2);
        builder.Property(b => b.Notes).HasMaxLength(500);

        builder.HasOne(b => b.Product).WithMany(p => p.Batches).HasForeignKey(b => b.ProductId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => b.ProductId);
        builder.HasIndex(b => b.CompanyId);
        builder.HasIndex(b => new { b.ProductId, b.BatchNumber });
    }
}
