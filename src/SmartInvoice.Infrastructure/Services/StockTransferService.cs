using Microsoft.EntityFrameworkCore;
using SmartInvoice.Application.Common;
using SmartInvoice.Application.Inventory;
using SmartInvoice.Application.Inventory.DTOs;
using SmartInvoice.Domain.Entities;
using SmartInvoice.Domain.Enums;
using SmartInvoice.Infrastructure.Persistence;

namespace SmartInvoice.Infrastructure.Services;

public class StockTransferService : IStockTransferService
{
    private readonly AppDbContext _context;
    private readonly IInventoryService _inventoryService;

    public StockTransferService(AppDbContext context, IInventoryService inventoryService)
    {
        _context = context;
        _inventoryService = inventoryService;
    }

    public async Task<Result<StockTransferResponse>> CreateAsync(CreateStockTransferRequest request)
    {
        if (request.FromWarehouseId == request.ToWarehouseId)
            return Result<StockTransferResponse>.Failure("From and To warehouse cannot be the same.");

        var transfer = new StockTransfer
        {
            FromWarehouseId = request.FromWarehouseId,
            ToWarehouseId = request.ToWarehouseId,
            TransferNumber = $"TRF-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}",
            Date = DateTime.UtcNow,
            Status = StockTransferStatus.Draft,
            Notes = request.Notes
        };

        foreach (var item in request.Items)
        {
            transfer.Items.Add(new StockTransferItem
            {
                ProductId = item.ProductId,
                BatchId = item.BatchId,
                Quantity = item.Quantity
            });
        }

        _context.StockTransfers.Add(transfer);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(transfer.Id);
    }

    public async Task<IReadOnlyList<StockTransferResponse>> GetAllAsync()
    {
        var transfers = await _context.StockTransfers
            .Include(t => t.FromWarehouse)
            .Include(t => t.ToWarehouse)
            .Include(t => t.Items).ThenInclude(i => i.Product)
            .OrderByDescending(t => t.Date)
            .ToListAsync();

        return transfers.Select(Map).ToList().AsReadOnly();
    }

    public async Task<Result<StockTransferResponse>> CompleteAsync(Guid transferId)
    {
        var transfer = await _context.StockTransfers
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == transferId);

        if (transfer is null)
            return Result<StockTransferResponse>.Failure("Transfer not found.");

        if (transfer.Status != StockTransferStatus.Draft && transfer.Status != StockTransferStatus.InTransit)
            return Result<StockTransferResponse>.Failure("Transfer cannot be completed in its current status.");

        // Create stock entries for each item
        foreach (var item in transfer.Items)
        {
            // Out from source
            await _inventoryService.RecordStockEntryAsync(
                item.ProductId, transfer.FromWarehouseId, item.Quantity,
                StockEntryType.Transfer, "StockTransfer", transfer.Id, item.BatchId,
                $"Transfer out to {transfer.ToWarehouseId}");

            // In to destination (record as In type)
            var entryIn = new StockEntry
            {
                ProductId = item.ProductId,
                WarehouseId = transfer.ToWarehouseId,
                BatchId = item.BatchId,
                Quantity = item.Quantity,
                Type = StockEntryType.In,
                ReferenceType = "StockTransfer",
                ReferenceId = transfer.Id,
                Notes = $"Transfer in from {transfer.FromWarehouseId}",
                Date = DateTime.UtcNow
            };
            _context.StockEntries.Add(entryIn);
        }

        transfer.Status = StockTransferStatus.Completed;
        await _context.SaveChangesAsync();

        return await GetByIdAsync(transfer.Id);
    }

    public async Task<Result<bool>> CancelAsync(Guid transferId)
    {
        var transfer = await _context.StockTransfers.FirstOrDefaultAsync(t => t.Id == transferId);
        if (transfer is null) return Result<bool>.Failure("Transfer not found.");

        if (transfer.Status == StockTransferStatus.Completed)
            return Result<bool>.Failure("Completed transfers cannot be cancelled.");

        transfer.Status = StockTransferStatus.Cancelled;
        await _context.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    private async Task<Result<StockTransferResponse>> GetByIdAsync(Guid id)
    {
        var transfer = await _context.StockTransfers
            .Include(t => t.FromWarehouse)
            .Include(t => t.ToWarehouse)
            .Include(t => t.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transfer is null) return Result<StockTransferResponse>.Failure("Transfer not found.");
        return Result<StockTransferResponse>.Success(Map(transfer));
    }

    private static StockTransferResponse Map(StockTransfer t) => new(
        t.Id, t.TransferNumber,
        t.FromWarehouseId, t.FromWarehouse.Name,
        t.ToWarehouseId, t.ToWarehouse.Name,
        t.Date, t.Status, t.Notes,
        t.Items.Select(i => new StockTransferItemResponse(i.ProductId, i.Product.Name, i.BatchId, i.Quantity)).ToList()
    );
}
