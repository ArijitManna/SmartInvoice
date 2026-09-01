using SmartInvoice.Application.ImportExport.DTOs;

namespace SmartInvoice.Application.ImportExport;

public interface IImportExportService
{
    // Imports (accept raw file stream from an .xlsx or .csv upload)
    Task<ImportResult> ImportProductsAsync(Stream fileStream, string fileName);
    Task<ImportResult> ImportCustomersAsync(Stream fileStream, string fileName);
    Task<ImportResult> ImportVendorsAsync(Stream fileStream, string fileName);
    Task<ImportResult> ImportOpeningStockAsync(Stream fileStream, string fileName);

    // Exports
    Task<ExportFile> ExportProductsAsync(ExportFormat format);
    Task<ExportFile> ExportCustomersAsync(ExportFormat format);
    Task<ExportFile> ExportVendorsAsync(ExportFormat format);
    Task<ExportFile> ExportInvoicesAsync(ExportFormat format);

    // Blank templates (always Excel)
    ExportFile GetProductTemplate();
    ExportFile GetCustomerTemplate();
    ExportFile GetVendorTemplate();
    ExportFile GetOpeningStockTemplate();
}
