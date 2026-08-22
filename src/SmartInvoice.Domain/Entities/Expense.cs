using SmartInvoice.Domain.Common;
using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Domain.Entities;

public class Expense : BaseEntity
{
    public Guid CategoryId { get; set; }
    public Guid? VendorId { get; set; }
    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public PaymentMode PaymentMode { get; set; }
    public ExpenseStatus Status { get; set; } = ExpenseStatus.Pending;

    // Recurring
    public bool IsRecurring { get; set; }
    public RecurrenceFrequency? RecurrenceFrequency { get; set; }
    public DateTime? NextDueDate { get; set; }

    // Receipt
    public string? ReceiptUrl { get; set; }

    // Navigation
    public ExpenseCategory Category { get; set; } = null!;
    public Vendor? Vendor { get; set; }
}
