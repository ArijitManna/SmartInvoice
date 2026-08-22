using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInvoice.API.Filters;
using SmartInvoice.Application.Invoices;
using SmartInvoice.Application.Invoices.DTOs;

namespace SmartInvoice.API.Controllers;

[ApiController]
[Route("api/recurring-invoices")]
[Authorize]
public class RecurringInvoiceController : ControllerBase
{
    private readonly IRecurringInvoiceService _recurringService;

    public RecurringInvoiceController(IRecurringInvoiceService recurringService)
    {
        _recurringService = recurringService;
    }

    [HttpPost]
    [RequirePermission("Invoice.Create")]
    public async Task<IActionResult> Create([FromBody] CreateRecurringInvoiceRequest request)
    {
        var result = await _recurringService.CreateAsync(request);
        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });
        return Created("", result.Value);
    }

    [HttpGet]
    [RequirePermission("Invoice.View")]
    public async Task<IActionResult> GetAll()
    {
        var items = await _recurringService.GetAllAsync();
        return Ok(items);
    }

    [HttpPost("{id:guid}/deactivate")]
    [RequirePermission("Invoice.Edit")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await _recurringService.DeactivateAsync(id);
        if (!result.IsSuccess) return NotFound(new { Error = result.Error });
        return Ok(new { Message = "Recurring invoice deactivated." });
    }

    [HttpPost("process")]
    [RequirePermission("Invoice.Create")]
    public async Task<IActionResult> ProcessNow()
    {
        var count = await _recurringService.ProcessDueRecurringInvoicesAsync();
        return Ok(new { Message = $"Generated {count} invoice(s) from recurring templates." });
    }
}
