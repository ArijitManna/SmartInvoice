using SmartInvoice.Domain.Common;
using SmartInvoice.Domain.Enums;
using SmartInvoice.Domain.ValueObjects;

namespace SmartInvoice.Domain.Entities;

public class Invoice : AuditableEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public InvoiceType Type { get; set; } = InvoiceType.Regular;
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }

    // Customer
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    // Amounts
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal CgstAmount { get; set; }
    public decimal SgstAmount { get; set; }
    public decimal IgstAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal BalanceDue { get; set; }
    public string Currency { get; set; } = "INR";

    // GST
    public GstType GstType { get; set; }

    // Additional
    public string? Notes { get; set; }
    public string? TermsAndConditions { get; set; }
    public string? ReferenceNumber { get; set; }

    // Enhancements
    public Guid? QuotationId { get; set; }
    public Guid? TemplateId { get; set; }
    public string? DeliveryChallanNumber { get; set; }
    public decimal AdvanceAmount { get; set; }

    // Navigation
    public ICollection<InvoiceItem> Items { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];

    /// <summary>
    /// Recalculates all totals from line items.
    /// </summary>
    public void Recalculate(string supplierStateCode, string customerStateCode)
    {
        SubTotal = Items.Sum(i => i.Amount);

        decimal discountedSubTotal = SubTotal;
        if (DiscountPercentage > 0)
        {
            DiscountAmount = Math.Round(SubTotal * DiscountPercentage / 100, 2, MidpointRounding.AwayFromZero);
            discountedSubTotal = SubTotal - DiscountAmount;
        }
        else if (DiscountAmount > 0)
        {
            discountedSubTotal = SubTotal - DiscountAmount;
        }

        // Determine GST type based on state comparison
        bool isSameState = string.Equals(supplierStateCode, customerStateCode, StringComparison.OrdinalIgnoreCase);
        GstType = isSameState ? GstType.IntraState : GstType.InterState;

        // Calculate tax from items
        TaxAmount = Items.Sum(i => i.TaxAmount);

        if (GstType == GstType.IntraState)
        {
            CgstAmount = Math.Round(TaxAmount / 2, 2, MidpointRounding.AwayFromZero);
            SgstAmount = TaxAmount - CgstAmount; // Ensures no rounding loss
            IgstAmount = 0;
        }
        else
        {
            IgstAmount = TaxAmount;
            CgstAmount = 0;
            SgstAmount = 0;
        }

        TotalAmount = Math.Round(discountedSubTotal + TaxAmount, 2, MidpointRounding.AwayFromZero);
        BalanceDue = TotalAmount - AmountPaid;
    }

    /// <summary>
    /// Records a payment and updates the invoice status accordingly.
    /// </summary>
    public void RecordPayment(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Payment amount must be positive.", nameof(amount));

        if (amount > BalanceDue)
            throw new InvalidOperationException($"Payment amount {amount} exceeds balance due {BalanceDue}.");

        AmountPaid += amount;
        BalanceDue = TotalAmount - AmountPaid;

        Status = BalanceDue <= 0 ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
    }

    /// <summary>
    /// Marks the invoice as cancelled.
    /// </summary>
    public void Cancel()
    {
        if (Status == InvoiceStatus.Paid)
            throw new InvalidOperationException("Cannot cancel a fully paid invoice.");

        Status = InvoiceStatus.Cancelled;
    }
}
