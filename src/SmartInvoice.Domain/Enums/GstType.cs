namespace SmartInvoice.Domain.Enums;

/// <summary>
/// Determines GST calculation mode based on supplier/customer state comparison.
/// </summary>
public enum GstType
{
    /// <summary>Intra-state: CGST + SGST (split 50/50)</summary>
    IntraState = 0,

    /// <summary>Inter-state: IGST (full rate)</summary>
    InterState = 1
}
