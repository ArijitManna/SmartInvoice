namespace SmartInvoice.Domain.ValueObjects;

/// <summary>
/// Represents GST registration details for a company or customer.
/// </summary>
public record GstInfo
{
    public string? Gstin { get; init; }
    public string? Pan { get; init; }
    public string? StateCode { get; init; }

    /// <summary>
    /// Validates the GSTIN format (15 characters: 2-digit state code + PAN + entity code + check digit).
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(Gstin))
            return true; // Optional field

        return Gstin.Length == 15;
    }

    /// <summary>
    /// Extracts the 2-digit state code from the GSTIN.
    /// </summary>
    public string? GetStateCodeFromGstin()
    {
        if (string.IsNullOrWhiteSpace(Gstin) || Gstin.Length < 2)
            return null;

        return Gstin[..2];
    }
}
