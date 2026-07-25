namespace SmartInvoice.Application.Categories.DTOs;

public record CreateCategoryRequest(
    string Name,
    string? Description,
    Guid? ParentCategoryId
);
