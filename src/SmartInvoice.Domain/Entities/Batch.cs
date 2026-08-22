using SmartInvoice.Domain.Common;

namespace SmartInvoice.Domain.Entities;

public class Batch : BaseEntity
{
    public Guid ProductId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime? ManufactureDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Product Product { get; set; } = null!;
    public ICollection<StockEntry> StockEntries { get; set; } = [];
}
