using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInvoice.Domain.Entities;

namespace SmartInvoice.Infrastructure.Persistence.Configurations;

public class StockTransferItemConfiguration : IEntityTypeConfiguration<StockTransferItem>
{
    public void Configure(EntityTypeBuilder<StockTransferItem> builder)
    {
        builder.ToTable("StockTransferItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Quantity).HasPrecision(18, 4);

        builder.HasOne(i => i.Transfer).WithMany(t => t.Items).HasForeignKey(i => i.TransferId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(i => i.Batch).WithMany().HasForeignKey(i => i.BatchId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(i => i.TransferId);
    }
}
