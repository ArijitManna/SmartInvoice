namespace SmartInvoice.Domain.ValueObjects;

public record Address
{
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = "India";

    /// <summary>
    /// Returns the state code used for GST inter/intra state determination.
    /// </summary>
    public string GetStateCode() => State.ToUpperInvariant().Trim();
}
