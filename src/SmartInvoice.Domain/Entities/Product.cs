using SmartInvoice.Domain.Common;
using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Domain.Entities;

public class Product : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProductType Type { get; set; } = ProductType.Product;
    public string? Sku { get; set; }
    public string? HsnSacCode { get; set; }
    public string Unit { get; set; } = "Nos";
    public decimal Price { get; set; }
    public decimal TaxRate { get; set; }
    public Guid? CategoryId { get; set; }

    // Navigation
    public Category? Category { get; set; }
}
