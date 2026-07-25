using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Application.Products.DTOs;

public record CreateProductRequest(
    string Name,
    string? Description,
    ProductType Type,
    string? Sku,
    string? HsnSacCode,
    string Unit,
    decimal Price,
    decimal TaxRate,
    Guid? CategoryId
);
