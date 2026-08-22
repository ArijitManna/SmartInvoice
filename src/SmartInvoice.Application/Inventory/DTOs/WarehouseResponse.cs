namespace SmartInvoice.Application.Inventory.DTOs;

public record WarehouseRequest(
    string Name,
    string? Code,
    string? Street,
    string? City,
    string? State,
    string? PostalCode,
    string? Country,
    string? ContactPerson,
    string? Phone,
    bool IsDefault
);

public record WarehouseResponse(
    Guid Id,
    string Name,
    string? Code,
    string? Street,
    string? City,
    string? State,
    string? PostalCode,
    string? Country,
    string? ContactPerson,
    string? Phone,
    bool IsDefault
);
