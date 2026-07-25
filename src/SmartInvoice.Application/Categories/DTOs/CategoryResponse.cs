namespace SmartInvoice.Application.Categories.DTOs;

public record CategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    string? ParentCategoryName
);
