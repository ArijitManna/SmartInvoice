using System.Text.RegularExpressions;

namespace SmartInvoice.Application.Common;

/// <summary>
/// Validates Indian GSTIN (Goods and Services Tax Identification Number).
/// Format: 2-digit state code + 10-char PAN + 1 entity code + 1 'Z' + 1 check digit = 15 chars.
/// Example: 29ABCDE1234F1Z5
/// </summary>
public static partial class GstinValidator
{
    // Pattern: 2 digits (01-37) + 5 uppercase letters + 4 digits + 1 uppercase letter + 1 alphanumeric + Z + 1 alphanumeric
    private static readonly Regex GstinRegex = CreateGstinRegex();

    [GeneratedRegex(@"^[0-3][0-9][A-Z]{5}[0-9]{4}[A-Z][0-9A-Z]Z[0-9A-Z]$", RegexOptions.Compiled)]
    private static partial Regex CreateGstinRegex();

    /// <summary>
    /// Valid Indian state codes for GSTIN (01 to 37 + 97 for other territory).
    /// </summary>
    private static readonly HashSet<string> ValidStateCodes =
    [
        "01", "02", "03", "04", "05", "06", "07", "08", "09", "10",
        "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
        "21", "22", "23", "24", "25", "26", "27", "28", "29", "30",
        "31", "32", "33", "34", "35", "36", "37", "97"
    ];

    /// <summary>
    /// Validates a GSTIN string. Returns null if valid, or an error message if invalid.
    /// Empty/null GSTIN is considered valid (optional field).
    /// </summary>
    public static string? Validate(string? gstin)
    {
        if (string.IsNullOrWhiteSpace(gstin))
            return null; // Optional field

        gstin = gstin.Trim().ToUpperInvariant();

        if (gstin.Length != 15)
            return "GSTIN must be exactly 15 characters.";

        if (!GstinRegex.IsMatch(gstin))
            return "GSTIN format is invalid. Expected format: 29ABCDE1234F1Z5.";

        string stateCode = gstin[..2];
        if (!ValidStateCodes.Contains(stateCode))
            return $"Invalid state code '{stateCode}' in GSTIN.";

        // Validate check digit (simplified Luhn-like algorithm)
        if (!ValidateCheckDigit(gstin))
            return "GSTIN check digit is invalid.";

        return null; // Valid
    }

    /// <summary>
    /// Quick check: is this GSTIN format valid? (true = valid or empty)
    /// </summary>
    public static bool IsValid(string? gstin)
    {
        return Validate(gstin) is null;
    }

    /// <summary>
    /// Extract the 2-digit state code from a GSTIN.
    /// </summary>
    public static string? GetStateCode(string? gstin)
    {
        if (string.IsNullOrWhiteSpace(gstin) || gstin.Length < 2)
            return null;
        return gstin[..2];
    }

    /// <summary>
    /// Extract the PAN from a GSTIN (characters 3-12).
    /// </summary>
    public static string? GetPan(string? gstin)
    {
        if (string.IsNullOrWhiteSpace(gstin) || gstin.Length < 12)
            return null;
        return gstin[2..12];
    }

    private static bool ValidateCheckDigit(string gstin)
    {
        // GSTIN check digit validation using the standard algorithm
        const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        int sum = 0;

        for (int i = 0; i < 14; i++)
        {
            int idx = chars.IndexOf(gstin[i]);
            if (idx < 0) return false;

            int product = ((i % 2 == 0) ? 1 : 2) * idx;
            sum += product / 36 + product % 36;
        }

        int remainder = sum % 36;
        int checkValue = (36 - remainder) % 36;
        char expectedCheck = chars[checkValue];

        return gstin[14] == expectedCheck;
    }
}
