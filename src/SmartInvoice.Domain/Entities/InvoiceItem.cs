using SmartInvoice.Domain.Common;

namespace SmartInvoice.Domain.Entities;

public class InvoiceItem : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public Guid? ProductId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? HsnSacCode { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "Nos";
    public decimal Rate { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Amount { get; set; }

    // Navigation
    public Invoice Invoice { get; set; } = null!;
    public Product? Product { get; set; }

    /// <summary>
    /// Calculates Amount and TaxAmount from Quantity, Rate, Discount, and TaxRate.
    /// </summary>
    public void Calculate()
    {
        decimal lineTotal = Quantity * Rate;

        if (DiscountPercentage > 0)
        {
            DiscountAmount = Math.Round(lineTotal * DiscountPercentage / 100, 2, MidpointRounding.AwayFromZero);
        }

        decimal taxableAmount = lineTotal - DiscountAmount;
        TaxAmount = Math.Round(taxableAmount * TaxRate / 100, 2, MidpointRounding.AwayFromZero);
        Amount = taxableAmount;
    }
}
