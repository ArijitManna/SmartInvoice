using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInvoice.API.Filters;
using SmartInvoice.Application.Inventory;
using SmartInvoice.Application.Inventory.DTOs;

namespace SmartInvoice.API.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly IStockTransferService _transferService;

    public InventoryController(IInventoryService inventoryService, IStockTransferService transferService)
    {
        _inventoryService = inventoryService;
        _transferService = transferService;
    }

    // --- Stock Levels ---

    [HttpGet("stock")]
    [RequirePermission("Inventory.View")]
    public async Task<IActionResult> GetStockLevels([FromQuery] Guid? productId = null, [FromQuery] Guid? warehouseId = null)
    {
        var levels = await _inventoryService.GetStockLevelsAsync(productId, warehouseId);
        return Ok(levels);
    }

    [HttpGet("summary")]
    [RequirePermission("Inventory.View")]
    public async Task<IActionResult> GetStockSummary()
    {
        var summary = await _inventoryService.GetStockSummaryAsync();
        return Ok(summary);
    }

    [HttpGet("low-stock")]
    [RequirePermission("Inventory.View")]
    public async Task<IActionResult> GetLowStockAlerts()
    {
        var alerts = await _inventoryService.GetLowStockAlertsAsync();
        return Ok(alerts);
    }

    // --- Stock Adjustments ---

    [HttpPost("adjustments")]
    [RequirePermission("Inventory.Manage")]
    public async Task<IActionResult> AdjustStock([FromBody] StockAdjustmentRequest request)
    {
        await _inventoryService.AdjustStockAsync(request);
        return Ok(new { Message = "Stock adjusted successfully." });
    }

    // --- Stock Transfers ---

    [HttpGet("transfers")]
    [RequirePermission("Inventory.View")]
    public async Task<IActionResult> GetTransfers()
    {
        var transfers = await _transferService.GetAllAsync();
        return Ok(transfers);
    }

    [HttpPost("transfers")]
    [RequirePermission("Inventory.Manage")]
    public async Task<IActionResult> CreateTransfer([FromBody] CreateStockTransferRequest request)
    {
        var result = await _transferService.CreateAsync(request);
        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });
        return Created("", result.Value);
    }

    [HttpPost("transfers/{id:guid}/complete")]
    [RequirePermission("Inventory.Manage")]
    public async Task<IActionResult> CompleteTransfer(Guid id)
    {
        var result = await _transferService.CompleteAsync(id);
        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });
        return Ok(result.Value);
    }

    [HttpPost("transfers/{id:guid}/cancel")]
    [RequirePermission("Inventory.Manage")]
    public async Task<IActionResult> CancelTransfer(Guid id)
    {
        var result = await _transferService.CancelAsync(id);
        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });
        return NoContent();
    }
}
