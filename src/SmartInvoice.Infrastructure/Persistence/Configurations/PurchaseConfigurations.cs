using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInvoice.Domain.Entities;

namespace SmartInvoice.Infrastructure.Persistence.Configurations;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.PONumber).IsRequired().HasMaxLength(50);
        builder.Property(p => p.SubTotal).HasPrecision(18, 2);
        builder.Property(p => p.TaxAmount).HasPrecision(18, 2);
        builder.Property(p => p.TotalAmount).HasPrecision(18, 2);
        builder.Property(p => p.Notes).HasMaxLength(2000);
        builder.Property(p => p.Terms).HasMaxLength(5000);
        builder.HasOne(p => p.Vendor).WithMany(v => v.PurchaseOrders).HasForeignKey(p => p.VendorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(p => p.CompanyId);
        builder.HasIndex(p => new { p.CompanyId, p.PONumber }).IsUnique();
    }
}

public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.ToTable("PurchaseOrderItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Description).HasMaxLength(500);
        builder.Property(i => i.Unit).HasMaxLength(20);
        builder.Property(i => i.Quantity).HasPrecision(18, 4);
        builder.Property(i => i.Rate).HasPrecision(18, 2);
        builder.Property(i => i.TaxRate).HasPrecision(5, 2);
        builder.Property(i => i.TaxAmount).HasPrecision(18, 2);
        builder.Property(i => i.Amount).HasPrecision(18, 2);
        builder.HasOne(i => i.PurchaseOrder).WithMany(p => p.Items).HasForeignKey(i => i.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PurchaseBillConfiguration : IEntityTypeConfiguration<PurchaseBill>
{
    public void Configure(EntityTypeBuilder<PurchaseBill> builder)
    {
        builder.ToTable("PurchaseBills");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.BillNumber).IsRequired().HasMaxLength(50);
        builder.Property(b => b.SubTotal).HasPrecision(18, 2);
        builder.Property(b => b.TaxAmount).HasPrecision(18, 2);
        builder.Property(b => b.TotalAmount).HasPrecision(18, 2);
        builder.Property(b => b.AmountPaid).HasPrecision(18, 2);
        builder.Property(b => b.BalanceDue).HasPrecision(18, 2);
        builder.Property(b => b.Notes).HasMaxLength(2000);
        builder.HasOne(b => b.Vendor).WithMany(v => v.PurchaseBills).HasForeignKey(b => b.VendorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(b => b.PurchaseOrder).WithMany().HasForeignKey(b => b.PurchaseOrderId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(b => b.CompanyId);
    }
}

public class PurchaseBillItemConfiguration : IEntityTypeConfiguration<PurchaseBillItem>
{
    public void Configure(EntityTypeBuilder<PurchaseBillItem> builder)
    {
        builder.ToTable("PurchaseBillItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Description).HasMaxLength(500);
        builder.Property(i => i.Unit).HasMaxLength(20);
        builder.Property(i => i.Quantity).HasPrecision(18, 4);
        builder.Property(i => i.Rate).HasPrecision(18, 2);
        builder.Property(i => i.TaxRate).HasPrecision(5, 2);
        builder.Property(i => i.TaxAmount).HasPrecision(18, 2);
        builder.Property(i => i.Amount).HasPrecision(18, 2);
        builder.HasOne(i => i.PurchaseBill).WithMany(b => b.Items).HasForeignKey(i => i.PurchaseBillId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PurchaseReturnConfiguration : IEntityTypeConfiguration<PurchaseReturn>
{
    public void Configure(EntityTypeBuilder<PurchaseReturn> builder)
    {
        builder.ToTable("PurchaseReturns");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Amount).HasPrecision(18, 2);
        builder.Property(r => r.Reason).HasMaxLength(500);
        builder.Property(r => r.Status).HasMaxLength(20);
        builder.HasOne(r => r.PurchaseBill).WithMany().HasForeignKey(r => r.PurchaseBillId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.Vendor).WithMany().HasForeignKey(r => r.VendorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(r => r.CompanyId);
    }
}

public class VendorLedgerEntryConfiguration : IEntityTypeConfiguration<VendorLedgerEntry>
{
    public void Configure(EntityTypeBuilder<VendorLedgerEntry> builder)
    {
        builder.ToTable("VendorLedgerEntries");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Debit).HasPrecision(18, 2);
        builder.Property(e => e.Credit).HasPrecision(18, 2);
        builder.Property(e => e.RunningBalance).HasPrecision(18, 2);
        builder.Property(e => e.ReferenceNumber).HasMaxLength(100);
        builder.Property(e => e.Notes).HasMaxLength(500);
        builder.HasOne(e => e.Vendor).WithMany(v => v.LedgerEntries).HasForeignKey(e => e.VendorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(e => e.VendorId);
        builder.HasIndex(e => e.CompanyId);
    }
}

public class VendorPaymentConfiguration : IEntityTypeConfiguration<VendorPayment>
{
    public void Configure(EntityTypeBuilder<VendorPayment> builder)
    {
        builder.ToTable("VendorPayments");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Amount).HasPrecision(18, 2);
        builder.Property(p => p.ReferenceNumber).HasMaxLength(100);
        builder.Property(p => p.Notes).HasMaxLength(500);
        builder.HasOne(p => p.Vendor).WithMany(v => v.Payments).HasForeignKey(p => p.VendorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.PurchaseBill).WithMany(b => b.Payments).HasForeignKey(p => p.PurchaseBillId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(p => p.VendorId);
        builder.HasIndex(p => p.CompanyId);
    }
}
