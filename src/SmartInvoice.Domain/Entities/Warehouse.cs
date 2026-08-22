using SmartInvoice.Domain.Common;

namespace SmartInvoice.Domain.Entities;

public class Warehouse : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; } = "India";
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public bool IsDefault { get; set; }

    // Navigation
    public ICollection<StockEntry> StockEntries { get; set; } = [];
}
