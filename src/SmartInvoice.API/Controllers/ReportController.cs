using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInvoice.Application.Reports;

namespace SmartInvoice.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("sales")]
    public async Task<IActionResult> GetSalesReport(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] Guid? customerId = null)
    {
        var report = await _reportService.GetSalesReportAsync(from, to, customerId);
        return Ok(report);
    }

    [HttpGet("outstanding")]
    public async Task<IActionResult> GetOutstandingReport()
    {
        var report = await _reportService.GetOutstandingReportAsync();
        return Ok(report);
    }

    [HttpGet("gst")]
    public async Task<IActionResult> GetGstReport(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var report = await _reportService.GetGstReportAsync(from, to);
        return Ok(report);
    }
}
