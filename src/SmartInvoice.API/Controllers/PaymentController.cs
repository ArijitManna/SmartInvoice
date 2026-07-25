using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInvoice.Application.Payments;
using SmartInvoice.Application.Payments.DTOs;

namespace SmartInvoice.API.Controllers;

[ApiController]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("api/invoices/{invoiceId:guid}/payments")]
    public async Task<IActionResult> RecordPayment(Guid invoiceId, [FromBody] RecordPaymentRequest request)
    {
        var result = await _paymentService.RecordPaymentAsync(invoiceId, request);

        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error });
        }

        return Created("", result.Value);
    }

    [HttpGet("api/invoices/{invoiceId:guid}/payments")]
    public async Task<IActionResult> GetPaymentsByInvoice(Guid invoiceId)
    {
        var payments = await _paymentService.GetPaymentsByInvoiceAsync(invoiceId);
        return Ok(payments);
    }

    [HttpPost("api/payments/{paymentId:guid}/refund")]
    public async Task<IActionResult> Refund(Guid paymentId, [FromBody] RefundRequest? request = null)
    {
        var result = await _paymentService.RefundAsync(paymentId, request?.Notes);

        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error });
        }

        return Ok(result.Value);
    }
}

public record RefundRequest(string? Notes);
