namespace SmartInvoice.Application.Purchase.DTOs;

public record VendorRequest(
    string Name,
    string? Email,
    string? Phone,
    string? ContactPerson,
    string? Notes,
    string? Gstin,
    string? Pan,
    string? GstStateCode,
    string? Street,
    string? City,
    string? State,
    string? PostalCode,
    string? Country,
    string? BankName,
    string? AccountNumber,
    string? IfscCode,
    string? AccountHolderName,
    string? BranchName,
    string? UpiId
);

public record VendorResponse(
    Guid Id,
    string Name,
    string? Email,
    string? Phone,
    string? ContactPerson,
    string? Notes,
    decimal OutstandingBalance,
    string? Gstin,
    string? Pan,
    string? City,
    string? State,
    DateTime CreatedAt
);
