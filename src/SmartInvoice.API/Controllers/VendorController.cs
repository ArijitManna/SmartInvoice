using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInvoice.API.Filters;
using SmartInvoice.Application.Purchase;
using SmartInvoice.Application.Purchase.DTOs;

namespace SmartInvoice.API.Controllers;

[ApiController]
[Route("api/vendors")]
[Authorize]
public class VendorController : ControllerBase
{
    private readonly IVendorService _vendorService;

    public VendorController(IVendorService vendorService)
    {
        _vendorService = vendorService;
    }

    [HttpPost]
    [RequirePermission("Purchase.Create")]
    public async Task<IActionResult> Create([FromBody] VendorRequest request)
    {
        var result = await _vendorService.CreateAsync(request);
        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });
        return Created("", result.Value);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("Purchase.View")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _vendorService.GetByIdAsync(id);
        if (!result.IsSuccess) return NotFound(new { Error = result.Error });
        return Ok(result.Value);
    }

    [HttpGet]
    [RequirePermission("Purchase.View")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        var result = await _vendorService.GetAllAsync(page, pageSize, search);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("Purchase.Edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] VendorRequest request)
    {
        var result = await _vendorService.UpdateAsync(id, request);
        if (!result.IsSuccess) return NotFound(new { Error = result.Error });
        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("Purchase.Edit")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _vendorService.DeleteAsync(id);
        if (!result.IsSuccess) return NotFound(new { Error = result.Error });
        return NoContent();
    }
}
