using Microsoft.EntityFrameworkCore;
using SmartInvoice.Application.Common;
using SmartInvoice.Application.Products;
using SmartInvoice.Application.Products.DTOs;
using SmartInvoice.Domain.Entities;
using SmartInvoice.Infrastructure.Persistence;

namespace SmartInvoice.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ProductResponse>> CreateAsync(CreateProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            Sku = request.Sku,
            HsnSacCode = request.HsnSacCode,
            Unit = request.Unit,
            Price = request.Price,
            TaxRate = request.TaxRate,
            CategoryId = request.CategoryId
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Reload with category
        var saved = await _context.Products
            .Include(p => p.Category)
            .FirstAsync(p => p.Id == product.Id);

        return Result<ProductResponse>.Success(MapToResponse(saved));
    }

    public async Task<Result<ProductResponse>> GetByIdAsync(Guid id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            return Result<ProductResponse>.Failure("Product not found.");
        }

        return Result<ProductResponse>.Success(MapToResponse(product));
    }

    public async Task<PagedResult<ProductResponse>> GetAllAsync(
        int page = 1, int pageSize = 20, string? search = null, Guid? categoryId = null, string? sortBy = null, bool sortDesc = false)
    {
        var query = _context.Products.Include(p => p.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            string searchLower = search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(searchLower) ||
                (p.Sku != null && p.Sku.ToLower().Contains(searchLower)) ||
                (p.HsnSacCode != null && p.HsnSacCode.Contains(search)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        query = sortBy?.ToLower() switch
        {
            "name" => sortDesc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => sortDesc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "createdat" => sortDesc ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ProductResponse>(
            Items: items.Select(MapToResponse).ToList().AsReadOnly(),
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize
        );
    }

    public async Task<Result<ProductResponse>> UpdateAsync(Guid id, UpdateProductRequest request)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            return Result<ProductResponse>.Failure("Product not found.");
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Type = request.Type;
        product.Sku = request.Sku;
        product.HsnSacCode = request.HsnSacCode;
        product.Unit = request.Unit;
        product.Price = request.Price;
        product.TaxRate = request.TaxRate;
        product.CategoryId = request.CategoryId;

        await _context.SaveChangesAsync();

        // Reload category if changed
        if (request.CategoryId.HasValue && product.Category is null)
        {
            await _context.Entry(product).Reference(p => p.Category).LoadAsync();
        }

        return Result<ProductResponse>.Success(MapToResponse(product));
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            return Result<bool>.Failure("Product not found.");
        }

        product.IsDeleted = true;
        await _context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    private static ProductResponse MapToResponse(Product p)
    {
        return new ProductResponse(
            Id: p.Id,
            Name: p.Name,
            Description: p.Description,
            Type: p.Type,
            Sku: p.Sku,
            HsnSacCode: p.HsnSacCode,
            Unit: p.Unit,
            Price: p.Price,
            TaxRate: p.TaxRate,
            CategoryId: p.CategoryId,
            CategoryName: p.Category?.Name,
            CreatedAt: p.CreatedAt
        );
    }
}
