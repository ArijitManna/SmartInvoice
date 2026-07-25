namespace SmartInvoice.Application.Customers.DTOs;

public record CreateCustomerRequest(
    string Name,
    string? Email,
    string? Phone,
    string? ContactPerson,
    string? Notes,
    // GST
    string? Gstin,
    string? Pan,
    string? GstStateCode,
    // Billing Address
    string? BillingStreet,
    string? BillingCity,
    string? BillingState,
    string? BillingPostalCode,
    string? BillingCountry,
    // Shipping Address
    string? ShippingStreet,
    string? ShippingCity,
    string? ShippingState,
    string? ShippingPostalCode,
    string? ShippingCountry
);
