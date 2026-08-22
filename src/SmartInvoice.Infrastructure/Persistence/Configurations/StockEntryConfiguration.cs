using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInvoice.Domain.Entities;

namespace SmartInvoice.Infrastructure.Persistence.Configurations;

public class StockEntryConfiguration : IEntityTypeConfiguration<StockEntry>
{
    public void Configure(EntityTypeBuilder<StockEntry> builder)
    {
        builder.ToTable("StockEntries");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Quantity).HasPrecision(18, 4);
        builder.Property(s => s.ReferenceType).HasMaxLength(50);
        builder.Property(s => s.Notes).HasMaxLength(500);

        builder.HasOne(s => s.Product).WithMany(p => p.StockEntries).HasForeignKey(s => s.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.Warehouse).WithMany(w => w.StockEntries).HasForeignKey(s => s.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.Batch).WithMany(b => b.StockEntries).HasForeignKey(s => s.BatchId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(s => s.ProductId);
        builder.HasIndex(s => s.WarehouseId);
        builder.HasIndex(s => s.CompanyId);
        builder.HasIndex(s => new { s.ProductId, s.WarehouseId });
    }
}
