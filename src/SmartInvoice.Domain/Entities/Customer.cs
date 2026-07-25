using SmartInvoice.Domain.Common;
using SmartInvoice.Domain.ValueObjects;

namespace SmartInvoice.Domain.Entities;

public class Customer : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? ContactPerson { get; set; }
    public string? Notes { get; set; }

    // Value Objects
    public GstInfo GstInfo { get; set; } = new();
    public Address BillingAddress { get; set; } = new();
    public Address? ShippingAddress { get; set; }

    // Navigation
    public ICollection<Invoice> Invoices { get; set; } = [];
}
