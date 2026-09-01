using SmartInvoice.Domain.Common;
using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Domain.Entities;

public class ImportJob : BaseEntity
{
    public ImportType Type { get; set; }
    public string FileName { get; set; } = string.Empty;
    public ImportStatus Status { get; set; } = ImportStatus.Pending;
    public int TotalRows { get; set; }
    public int SuccessRows { get; set; }
    public int ErrorRows { get; set; }
    public string? ErrorLog { get; set; }
    public DateTime? CompletedAt { get; set; }
}
