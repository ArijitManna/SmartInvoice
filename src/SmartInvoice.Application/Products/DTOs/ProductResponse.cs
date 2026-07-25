using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Application.Products.DTOs;

public record ProductResponse(
    Guid Id,
    string Name,
    string? Description,
    ProductType Type,
    string? Sku,
    string? HsnSacCode,
    string Unit,
    decimal Price,
    decimal TaxRate,
    Guid? CategoryId,
    string? CategoryName,
    DateTime CreatedAt
);
