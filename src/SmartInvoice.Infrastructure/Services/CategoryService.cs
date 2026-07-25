using Microsoft.EntityFrameworkCore;
using SmartInvoice.Application.Categories;
using SmartInvoice.Application.Categories.DTOs;
using SmartInvoice.Application.Common;
using SmartInvoice.Domain.Entities;
using SmartInvoice.Infrastructure.Persistence;

namespace SmartInvoice.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CategoryResponse>> CreateAsync(CreateCategoryRequest request)
    {
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
            ParentCategoryId = request.ParentCategoryId
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Load parent name if needed
        string? parentName = null;
        if (request.ParentCategoryId.HasValue)
        {
            var parent = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.ParentCategoryId.Value);
            parentName = parent?.Name;
        }

        return Result<CategoryResponse>.Success(new CategoryResponse(
            category.Id, category.Name, category.Description,
            category.ParentCategoryId, parentName));
    }

    public async Task<IReadOnlyList<CategoryResponse>> GetAllAsync()
    {
        var categories = await _context.Categories
            .Include(c => c.ParentCategory)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return categories.Select(c => new CategoryResponse(
            c.Id, c.Name, c.Description,
            c.ParentCategoryId, c.ParentCategory?.Name
        )).ToList().AsReadOnly();
    }

    public async Task<Result<CategoryResponse>> UpdateAsync(Guid id, CreateCategoryRequest request)
    {
        var category = await _context.Categories
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
        {
            return Result<CategoryResponse>.Failure("Category not found.");
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.ParentCategoryId = request.ParentCategoryId;

        await _context.SaveChangesAsync();

        // Reload parent
        if (request.ParentCategoryId.HasValue)
        {
            await _context.Entry(category).Reference(c => c.ParentCategory).LoadAsync();
        }

        return Result<CategoryResponse>.Success(new CategoryResponse(
            category.Id, category.Name, category.Description,
            category.ParentCategoryId, category.ParentCategory?.Name));
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
        {
            return Result<bool>.Failure("Category not found.");
        }

        category.IsDeleted = true;
        await _context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }
}
