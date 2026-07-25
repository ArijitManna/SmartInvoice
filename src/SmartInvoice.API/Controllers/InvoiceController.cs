using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInvoice.Application.Email;
using SmartInvoice.Application.Invoices;
using SmartInvoice.Application.Invoices.DTOs;
using SmartInvoice.Domain.Enums;

namespace SmartInvoice.API.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize]
public class InvoiceController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly IInvoicePdfService _pdfService;
    private readonly IBackgroundJobClient _backgroundJobs;

    public InvoiceController(IInvoiceService invoiceService, IInvoicePdfService pdfService, IBackgroundJobClient backgroundJobs)
    {
        _invoiceService = invoiceService;
        _pdfService = pdfService;
        _backgroundJobs = backgroundJobs;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceRequest request)
    {
        var result = await _invoiceService.CreateAsync(request);

        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _invoiceService.GetByIdAsync(id);

        if (!result.IsSuccess)
        {
            return NotFound(new { Error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] InvoiceStatus? status = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false)
    {
        var result = await _invoiceService.GetAllAsync(page, pageSize, status, customerId, from, to, sortBy, sortDesc);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInvoiceRequest request)
    {
        var result = await _invoiceService.UpdateAsync(id, request);

        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/duplicate")]
    public async Task<IActionResult> Duplicate(Guid id)
    {
        var result = await _invoiceService.DuplicateAsync(id);

        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> GetPdf(Guid id)
    {
        var invoiceResult = await _invoiceService.GetByIdAsync(id);
        if (!invoiceResult.IsSuccess)
        {
            return NotFound(new { Error = invoiceResult.Error });
        }

        var pdfBytes = await _pdfService.GenerateAsync(id);
        var fileName = $"{invoiceResult.Value!.InvoiceNumber}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }

    [HttpPost("{id:guid}/send")]
    public async Task<IActionResult> Send(Guid id, [FromBody] SendInvoiceRequest request)
    {
        var invoiceResult = await _invoiceService.GetByIdAsync(id);
        if (!invoiceResult.IsSuccess)
        {
            return NotFound(new { Error = invoiceResult.Error });
        }

        var email = request.Email ?? invoiceResult.Value!.CustomerName;

        _backgroundJobs.Enqueue<IEmailService>(svc => svc.SendInvoiceEmailAsync(id, email));

        return Ok(new { Message = "Invoice email queued for delivery." });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _invoiceService.DeleteAsync(id);

        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error });
        }

        return NoContent();
    }
}
