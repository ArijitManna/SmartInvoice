namespace SmartInvoice.Application.Companies.DTOs;

public record CompanyResponse(
    Guid Id,
    string Name,
    string? Phone,
    string? Email,
    string? Website,
    string? LogoUrl,
    string? SignatureUrl,
    string DefaultCurrency,
    string TimeZone,
    string? InvoicePrefix,
    int NextInvoiceNumber,
    // Address
    string? Street,
    string? City,
    string? State,
    string? PostalCode,
    string? Country,
    // GST
    string? Gstin,
    string? Pan,
    string? GstStateCode,
    // Bank
    string? BankName,
    string? AccountNumber,
    string? IfscCode,
    string? AccountHolderName,
    string? BranchName,
    string? UpiId
);
