using SmartInvoice.Domain.Common;
using SmartInvoice.Domain.ValueObjects;

namespace SmartInvoice.Domain.Entities;

public class Company : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? SignatureUrl { get; set; }
    public string? Website { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string DefaultCurrency { get; set; } = "INR";
    public string TimeZone { get; set; } = "Asia/Kolkata";
    public string? InvoicePrefix { get; set; } = "INV";
    public int NextInvoiceNumber { get; set; } = 1;

    // Value Objects
    public Address Address { get; set; } = new();
    public GstInfo GstInfo { get; set; } = new();
    public BankDetails BankDetails { get; set; } = new();

    // Navigation
    public ICollection<Customer> Customers { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
    public ICollection<Invoice> Invoices { get; set; } = [];

    /// <summary>
    /// Generates the next invoice number using the configured prefix and increments the counter.
    /// </summary>
    public string GenerateInvoiceNumber()
    {
        string number = $"{InvoicePrefix}-{DateTime.UtcNow.Year}-{NextInvoiceNumber:D4}";
        NextInvoiceNumber++;
        return number;
    }
}
