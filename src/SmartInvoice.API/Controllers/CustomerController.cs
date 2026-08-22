using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInvoice.API.Filters;
using SmartInvoice.Application.Customers;
using SmartInvoice.Application.Customers.DTOs;
using SmartInvoice.Application.Invoices;

namespace SmartInvoice.API.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ICustomerAddressService _addressService;
    private readonly ICustomerLedgerService _ledgerService;
    private readonly IInvoiceService _invoiceService;

    public CustomerController(ICustomerService customerService, ICustomerAddressService addressService, ICustomerLedgerService ledgerService, IInvoiceService invoiceService)
    {
        _customerService = customerService;
        _addressService = addressService;
        _ledgerService = ledgerService;
        _invoiceService = invoiceService;
    }

    [HttpPost]
    [RequirePermission("Customer.Create")]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request)
    {
        var result = await _customerService.CreateAsync(request);

        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("Customer.View")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _customerService.GetByIdAsync(id);

        if (!result.IsSuccess)
        {
            return NotFound(new { Error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpGet]
    [RequirePermission("Customer.View")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false)
    {
        var result = await _customerService.GetAllAsync(page, pageSize, search, sortBy, sortDesc);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("Customer.Edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest request)
    {
        var result = await _customerService.UpdateAsync(id, request);

        if (!result.IsSuccess)
        {
            return NotFound(new { Error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("Customer.Delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _customerService.DeleteAsync(id);

        if (!result.IsSuccess)
        {
            return NotFound(new { Error = result.Error });
        }

        return NoContent();
    }

    // --- Customer Addresses ---

    [HttpGet("{customerId:guid}/addresses")]
    [RequirePermission("Customer.View")]
    public async Task<IActionResult> GetAddresses(Guid customerId)
    {
        var addresses = await _addressService.GetAllAsync(customerId);
        return Ok(addresses);
    }

    [HttpPost("{customerId:guid}/addresses")]
    [RequirePermission("Customer.Edit")]
    public async Task<IActionResult> CreateAddress(Guid customerId, [FromBody] CustomerAddressRequest request)
    {
        var result = await _addressService.CreateAsync(customerId, request);
        if (!result.IsSuccess)
            return BadRequest(new { Error = result.Error });
        return Created("", result.Value);
    }

    [HttpPut("{customerId:guid}/addresses/{addressId:guid}")]
    [RequirePermission("Customer.Edit")]
    public async Task<IActionResult> UpdateAddress(Guid customerId, Guid addressId, [FromBody] CustomerAddressRequest request)
    {
        var result = await _addressService.UpdateAsync(customerId, addressId, request);
        if (!result.IsSuccess)
            return NotFound(new { Error = result.Error });
        return Ok(result.Value);
    }

    [HttpDelete("{customerId:guid}/addresses/{addressId:guid}")]
    [RequirePermission("Customer.Edit")]
    public async Task<IActionResult> DeleteAddress(Guid customerId, Guid addressId)
    {
        var result = await _addressService.DeleteAsync(customerId, addressId);
        if (!result.IsSuccess)
            return NotFound(new { Error = result.Error });
        return NoContent();
    }

    // --- Customer Ledger ---

    [HttpGet("{customerId:guid}/ledger")]
    [RequirePermission("Customer.View")]
    public async Task<IActionResult> GetLedger(Guid customerId, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var entries = await _ledgerService.GetLedgerAsync(customerId, from, to);
        return Ok(entries);
    }

    [HttpPost("{customerId:guid}/ledger/recalculate")]
    [RequirePermission("Customer.Edit")]
    public async Task<IActionResult> RecalculateLedger(Guid customerId)
    {
        await _ledgerService.RecalculateBalanceAsync(customerId);
        return Ok(new { Message = "Ledger recalculated successfully." });
    }

    // --- Customer Sales History ---

    [HttpGet("{customerId:guid}/sales-history")]
    [RequirePermission("Customer.View")]
    public async Task<IActionResult> GetSalesHistory(Guid customerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _invoiceService.GetAllAsync(page, pageSize, customerId: customerId);
        return Ok(result);
    }
}
