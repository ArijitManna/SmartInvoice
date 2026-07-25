using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Application.Products.DTOs;

public record UpdateProductRequest(
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
