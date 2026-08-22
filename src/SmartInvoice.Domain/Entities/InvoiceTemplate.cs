using SmartInvoice.Domain.Common;

namespace SmartInvoice.Domain.Entities;

/// <summary>
/// Represents a PDF template style for invoices.
/// TemplateKey is used by QuestPDF to select the rendering layout.
/// </summary>
public class InvoiceTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string TemplateKey { get; set; } = "classic";
    public bool IsDefault { get; set; }
    public string? Description { get; set; }
    public string? PreviewImageUrl { get; set; }
}
