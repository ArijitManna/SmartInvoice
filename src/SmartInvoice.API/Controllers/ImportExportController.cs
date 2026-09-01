using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInvoice.API.Filters;
using SmartInvoice.Application.ImportExport;
using SmartInvoice.Application.ImportExport.DTOs;

namespace SmartInvoice.API.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class ImportExportController : ControllerBase
{
    private readonly IImportExportService _service;

    public ImportExportController(IImportExportService service)
    {
        _service = service;
    }

    // ---------------------------------------------------------------------
    // IMPORTS
    // ---------------------------------------------------------------------

    [HttpPost("import/products")]
    [RequirePermission("Data.Import")]
    public Task<IActionResult> ImportProducts(IFormFile file) =>
        RunImport(file, (s, n) => _service.ImportProductsAsync(s, n));

    [HttpPost("import/customers")]
    [RequirePermission("Data.Import")]
    public Task<IActionResult> ImportCustomers(IFormFile file) =>
        RunImport(file, (s, n) => _service.ImportCustomersAsync(s, n));

    [HttpPost("import/vendors")]
    [RequirePermission("Data.Import")]
    public Task<IActionResult> ImportVendors(IFormFile file) =>
        RunImport(file, (s, n) => _service.ImportVendorsAsync(s, n));

    [HttpPost("import/stock")]
    [RequirePermission("Data.Import")]
    public Task<IActionResult> ImportOpeningStock(IFormFile file) =>
        RunImport(file, (s, n) => _service.ImportOpeningStockAsync(s, n));

    private async Task<IActionResult> RunImport(IFormFile file, Func<Stream, string, Task<ImportResult>> importFunc)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { Error = "No file uploaded." });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not (".xlsx" or ".csv"))
            return BadRequest(new { Error = "Only .xlsx and .csv files are supported." });

        await using var stream = file.OpenReadStream();
        var result = await importFunc(stream, file.FileName);
        return Ok(result);
    }

    // ---------------------------------------------------------------------
    // EXPORTS
    // ---------------------------------------------------------------------

    [HttpGet("export/products")]
    [RequirePermission("Data.Export")]
    public async Task<IActionResult> ExportProducts([FromQuery] string format = "excel") =>
        Download(await _service.ExportProductsAsync(ParseFormat(format)));

    [HttpGet("export/customers")]
    [RequirePermission("Data.Export")]
    public async Task<IActionResult> ExportCustomers([FromQuery] string format = "excel") =>
        Download(await _service.ExportCustomersAsync(ParseFormat(format)));

    [HttpGet("export/vendors")]
    [RequirePermission("Data.Export")]
    public async Task<IActionResult> ExportVendors([FromQuery] string format = "excel") =>
        Download(await _service.ExportVendorsAsync(ParseFormat(format)));

    [HttpGet("export/invoices")]
    [RequirePermission("Data.Export")]
    public async Task<IActionResult> ExportInvoices([FromQuery] string format = "excel") =>
        Download(await _service.ExportInvoicesAsync(ParseFormat(format)));

    // ---------------------------------------------------------------------
    // TEMPLATES
    // ---------------------------------------------------------------------

    [HttpGet("import/templates/products")]
    [RequirePermission("Data.Import")]
    public IActionResult ProductTemplate() => Download(_service.GetProductTemplate());

    [HttpGet("import/templates/customers")]
    [RequirePermission("Data.Import")]
    public IActionResult CustomerTemplate() => Download(_service.GetCustomerTemplate());

    [HttpGet("import/templates/vendors")]
    [RequirePermission("Data.Import")]
    public IActionResult VendorTemplate() => Download(_service.GetVendorTemplate());

    [HttpGet("import/templates/stock")]
    [RequirePermission("Data.Import")]
    public IActionResult OpeningStockTemplate() => Download(_service.GetOpeningStockTemplate());

    // ---------------------------------------------------------------------
    // HELPERS
    // ---------------------------------------------------------------------

    private static ExportFormat ParseFormat(string format) =>
        format?.Trim().ToLowerInvariant() == "csv" ? ExportFormat.Csv : ExportFormat.Excel;

    private FileContentResult Download(ExportFile exportFile) =>
        File(exportFile.Content, exportFile.ContentType, exportFile.FileName);
}
