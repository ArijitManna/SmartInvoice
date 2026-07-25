namespace SmartInvoice.Application.Customers.DTOs;

public record UpdateCustomerRequest(
    string Name,
    string? Email,
    string? Phone,
    string? ContactPerson,
    string? Notes,
    string? Gstin,
    string? Pan,
    string? GstStateCode,
    string? BillingStreet,
    string? BillingCity,
    string? BillingState,
    string? BillingPostalCode,
    string? BillingCountry,
    string? ShippingStreet,
    string? ShippingCity,
    string? ShippingState,
    string? ShippingPostalCode,
    string? ShippingCountry
);
