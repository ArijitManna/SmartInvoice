using SmartInvoice.Domain.Common;

namespace SmartInvoice.Domain.Entities;

/// <summary>
/// Logs application errors for debugging and troubleshooting.
/// </summary>
public class ErrorLog : BaseEntity
{
    public string ErrorType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? InnerException { get; set; }
    public string SourceMethod { get; set; } = string.Empty;
    public string SourceFile { get; set; } = string.Empty;
    public int LineNumber { get; set; }

    // Context
    public string? UserId { get; set; }
    public string? RequestUrl { get; set; }
    public string? RequestMethod { get; set; }
    public int? HttpStatusCode { get; set; }
    public string? RequestPayload { get; set; }

    // Metadata
    public bool IsResolved { get; set; }
    public string? Resolution { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}
