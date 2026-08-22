using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInvoice.API.Filters;
using SmartInvoice.Application.Inventory;
using SmartInvoice.Application.Inventory.DTOs;

namespace SmartInvoice.API.Controllers;

[ApiController]
[Route("api/warehouses")]
[Authorize]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;

    public WarehouseController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    [HttpPost]
    [RequirePermission("Inventory.Manage")]
    public async Task<IActionResult> Create([FromBody] WarehouseRequest request)
    {
        var result = await _warehouseService.CreateAsync(request);
        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });
        return Created("", result.Value);
    }

    [HttpGet]
    [RequirePermission("Inventory.View")]
    public async Task<IActionResult> GetAll()
    {
        var warehouses = await _warehouseService.GetAllAsync();
        return Ok(warehouses);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("Inventory.Manage")]
    public async Task<IActionResult> Update(Guid id, [FromBody] WarehouseRequest request)
    {
        var result = await _warehouseService.UpdateAsync(id, request);
        if (!result.IsSuccess) return NotFound(new { Error = result.Error });
        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("Inventory.Manage")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _warehouseService.DeleteAsync(id);
        if (!result.IsSuccess) return NotFound(new { Error = result.Error });
        return NoContent();
    }
}
