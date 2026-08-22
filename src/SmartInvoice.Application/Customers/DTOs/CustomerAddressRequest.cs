using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Application.Customers.DTOs;

public record CustomerAddressRequest(
    AddressType Type,
    string Label,
    bool IsDefault,
    string Street,
    string City,
    string State,
    string PostalCode,
    string? Country
);
