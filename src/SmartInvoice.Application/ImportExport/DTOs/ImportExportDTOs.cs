using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Application.ImportExport.DTOs;

public record ImportResult(
    ImportType Type,
    int TotalRows,
    int SuccessRows,
    int ErrorRows,
    List<ImportRowError> Errors);

public record ImportRowError(int RowNumber, string Message);

public record ExportFile(byte[] Content, string FileName, string ContentType);

public enum ExportFormat
{
    Excel = 0,
    Csv = 1
}
