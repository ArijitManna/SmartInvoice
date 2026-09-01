using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInvoice.API.Filters;
using SmartInvoice.Application.Reports;

namespace SmartInvoice.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ExtendedReportController : ControllerBase
{
    private readonly IExtendedReportService _reportService;

    public ExtendedReportController(IExtendedReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("hsn-summary")]
    [RequirePermission("Report.View")]
    public async Task<IActionResult> GetHsnSummary([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var report = await _reportService.GetHsnSummaryAsync(from, to);
        return Ok(report);
    }

    [HttpGet("gstr1")]
    [RequirePermission("Report.View")]
    public async Task<IActionResult> GetGstr1([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var report = await _reportService.GetGstr1ReportAsync(from, to);
        return Ok(report);
    }

    [HttpGet("profit-loss")]
    [RequirePermission("Report.View")]
    public async Task<IActionResult> GetProfitLoss([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var report = await _reportService.GetProfitLossAsync(from, to);
        return Ok(report);
    }

    [HttpGet("cash-flow")]
    [RequirePermission("Report.View")]
    public async Task<IActionResult> GetCashFlow([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var report = await _reportService.GetCashFlowAsync(from, to);
        return Ok(report);
    }

    [HttpGet("products")]
    [RequirePermission("Report.View")]
    public async Task<IActionResult> GetProductReport([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var report = await _reportService.GetProductReportAsync(from, to);
        return Ok(report);
    }

    [HttpGet("yearly-revenue")]
    [RequirePermission("Report.View")]
    public async Task<IActionResult> GetYearlyRevenue([FromQuery] int? year = null)
    {
        var report = await _reportService.GetYearlyRevenueAsync(year ?? DateTime.UtcNow.Year);
        return Ok(report);
    }

    [HttpGet("tax-collected")]
    [RequirePermission("Report.View")]
    public async Task<IActionResult> GetTaxCollected([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var report = await _reportService.GetTaxCollectedAsync(from, to);
        return Ok(report);
    }
}
