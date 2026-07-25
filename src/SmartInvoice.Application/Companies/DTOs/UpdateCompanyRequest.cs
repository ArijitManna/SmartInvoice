namespace SmartInvoice.Application.Companies.DTOs;

public record UpdateCompanyRequest(
    string Name,
    string? Phone,
    string? Email,
    string? Website,
    string? LogoUrl,
    string? SignatureUrl,
    string? DefaultCurrency,
    string? TimeZone,
    string? InvoicePrefix,
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
