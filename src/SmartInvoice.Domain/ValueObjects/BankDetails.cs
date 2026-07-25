namespace SmartInvoice.Domain.ValueObjects;

/// <summary>
/// Represents bank account details for payment information on invoices.
/// </summary>
public record BankDetails
{
    public string BankName { get; init; } = string.Empty;
    public string AccountNumber { get; init; } = string.Empty;
    public string IfscCode { get; init; } = string.Empty;
    public string AccountHolderName { get; init; } = string.Empty;
    public string? BranchName { get; init; }
    public string? UpiId { get; init; }
}
