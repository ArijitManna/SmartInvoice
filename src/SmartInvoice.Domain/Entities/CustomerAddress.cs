using SmartInvoice.Domain.Common;
using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Domain.Entities;

public class CustomerAddress : BaseEntity
{
    public Guid CustomerId { get; set; }
    public AddressType Type { get; set; } = AddressType.Billing;
    public string Label { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = "India";

    // Navigation
    public Customer Customer { get; set; } = null!;

    public string GetStateCode() => State.ToUpperInvariant().Trim();
}
