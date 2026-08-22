using SmartInvoice.Domain.Common;
using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Domain.Entities;

public class RecurringInvoice : BaseEntity
{
    public Guid CustomerId { get; set; }
    public RecurrenceFrequency Frequency { get; set; }
    public DateTime NextGenerationDate { get; set; }
    public DateTime? LastGeneratedDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public string? TermsAndConditions { get; set; }
    public decimal DiscountPercentage { get; set; }
    public InvoiceType InvoiceType { get; set; } = InvoiceType.Regular;

    // Template items stored as JSON
    public string ItemsJson { get; set; } = "[]";

    // Navigation
    public Customer Customer { get; set; } = null!;
}
