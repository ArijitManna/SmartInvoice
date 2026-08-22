using Microsoft.EntityFrameworkCore;
using SmartInvoice.Application.Inventory;
using SmartInvoice.Application.Inventory.DTOs;
using SmartInvoice.Domain.Entities;
using SmartInvoice.Domain.Enums;
using SmartInvoice.Infrastructure.Persistence;

namespace SmartInvoice.Infrastructure.Services;

public class InventoryService : IInventoryService
{
    private readonly AppDbContext _context;

    public InventoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<StockLevelResponse>> GetStockLevelsAsync(Guid? productId = null, Guid? warehouseId = null)
    {
        var query = _context.StockEntries
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Where(s => s.Product.TrackInventory)
            .AsQueryable();

        if (productId.HasValue)
            query = query.Where(s => s.ProductId == productId.Value);

        if (warehouseId.HasValue)
            query = query.Where(s => s.WarehouseId == warehouseId.Value);

        var grouped = await query
            .GroupBy(s => new { s.ProductId, s.Product.Name, s.Product.Sku, s.WarehouseId, WarehouseName = s.Warehouse.Name, s.Product.LowStockThreshold })
            .Select(g => new
            {
                g.Key.ProductId,
                ProductName = g.Key.Name,
                g.Key.Sku,
                g.Key.WarehouseId,
                g.Key.WarehouseName,
                g.Key.LowStockThreshold,
                CurrentStock = g.Sum(s => s.Type == StockEntryType.In || s.Type == StockEntryType.Opening || s.Type == StockEntryType.Adjustment
                    ? s.Quantity
                    : -s.Quantity)
            })
            .ToListAsync();

        return grouped.Select(g => new StockLevelResponse(
            g.ProductId, g.ProductName, g.Sku, g.WarehouseId, g.WarehouseName,
            g.CurrentStock, g.LowStockThreshold,
            g.LowStockThreshold > 0 && g.CurrentStock <= g.LowStockThreshold
        )).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<StockSummaryResponse>> GetStockSummaryAsync()
    {
        var products = await _context.Products
            .Where(p => p.TrackInventory)
            .ToListAsync();

        var stockByProduct = await _context.StockEntries
            .GroupBy(s => s.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalStock = g.Sum(s => s.Type == StockEntryType.In || s.Type == StockEntryType.Opening || s.Type == StockEntryType.Adjustment
                    ? s.Quantity
                    : -s.Quantity)
            })
            .ToDictionaryAsync(x => x.ProductId, x => x.TotalStock);

        return products.Select(p =>
        {
            decimal stock = stockByProduct.GetValueOrDefault(p.Id, 0) + p.OpeningStock;
            return new StockSummaryResponse(
                p.Id, p.Name, p.Sku, p.Brand, stock,
                p.PurchasePrice, stock * p.PurchasePrice,
                p.LowStockThreshold, p.LowStockThreshold > 0 && stock <= p.LowStockThreshold
            );
        }).ToList().AsReadOnly();
    }

    public async Task RecordStockEntryAsync(Guid productId, Guid warehouseId, decimal quantity, StockEntryType type, string? referenceType = null, Guid? referenceId = null, Guid? batchId = null, string? notes = null)
    {
        var entry = new StockEntry
        {
            ProductId = productId,
            WarehouseId = warehouseId,
            BatchId = batchId,
            Quantity = Math.Abs(quantity),
            Type = type,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            Notes = notes,
            Date = DateTime.UtcNow
        };

        _context.StockEntries.Add(entry);
        await _context.SaveChangesAsync();
    }

    public async Task AdjustStockAsync(StockAdjustmentRequest request)
    {
        var type = request.Quantity >= 0 ? StockEntryType.In : StockEntryType.Out;
        // For adjustments we always use StockEntryType.Adjustment with signed quantity
        var entry = new StockEntry
        {
            ProductId = request.ProductId,
            WarehouseId = request.WarehouseId,
            Quantity = Math.Abs(request.Quantity),
            Type = request.Quantity >= 0 ? StockEntryType.Adjustment : StockEntryType.Out,
            Notes = request.Reason,
            Date = DateTime.UtcNow
        };

        _context.StockEntries.Add(entry);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<StockSummaryResponse>> GetLowStockAlertsAsync()
    {
        var summary = await GetStockSummaryAsync();
        return summary.Where(s => s.IsLowStock).ToList().AsReadOnly();
    }
}
