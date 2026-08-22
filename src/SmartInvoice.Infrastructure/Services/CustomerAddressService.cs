using Microsoft.EntityFrameworkCore;
using SmartInvoice.Application.Common;
using SmartInvoice.Application.Customers;
using SmartInvoice.Application.Customers.DTOs;
using SmartInvoice.Domain.Entities;
using SmartInvoice.Infrastructure.Persistence;

namespace SmartInvoice.Infrastructure.Services;

public class CustomerAddressService : ICustomerAddressService
{
    private readonly AppDbContext _context;

    public CustomerAddressService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CustomerAddressResponse>> GetAllAsync(Guid customerId)
    {
        var addresses = await _context.CustomerAddresses
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.Label)
            .ToListAsync();

        return addresses.Select(MapToResponse).ToList().AsReadOnly();
    }

    public async Task<Result<CustomerAddressResponse>> CreateAsync(Guid customerId, CustomerAddressRequest request)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == customerId);
        if (customer is null)
            return Result<CustomerAddressResponse>.Failure("Customer not found.");

        var address = new CustomerAddress
        {
            CustomerId = customerId,
            Type = request.Type,
            Label = request.Label,
            IsDefault = request.IsDefault,
            Street = request.Street,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country ?? "India"
        };

        // If this is marked as default, unmark existing defaults of the same type
        if (request.IsDefault)
        {
            var existingDefaults = await _context.CustomerAddresses
                .Where(a => a.CustomerId == customerId && a.Type == request.Type && a.IsDefault)
                .ToListAsync();

            foreach (var existing in existingDefaults)
            {
                existing.IsDefault = false;
            }
        }

        _context.CustomerAddresses.Add(address);
        await _context.SaveChangesAsync();

        return Result<CustomerAddressResponse>.Success(MapToResponse(address));
    }

    public async Task<Result<CustomerAddressResponse>> UpdateAsync(Guid customerId, Guid addressId, CustomerAddressRequest request)
    {
        var address = await _context.CustomerAddresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.CustomerId == customerId);

        if (address is null)
            return Result<CustomerAddressResponse>.Failure("Address not found.");

        address.Type = request.Type;
        address.Label = request.Label;
        address.IsDefault = request.IsDefault;
        address.Street = request.Street;
        address.City = request.City;
        address.State = request.State;
        address.PostalCode = request.PostalCode;
        address.Country = request.Country ?? "India";

        if (request.IsDefault)
        {
            var existingDefaults = await _context.CustomerAddresses
                .Where(a => a.CustomerId == customerId && a.Type == request.Type && a.IsDefault && a.Id != addressId)
                .ToListAsync();

            foreach (var existing in existingDefaults)
            {
                existing.IsDefault = false;
            }
        }

        await _context.SaveChangesAsync();

        return Result<CustomerAddressResponse>.Success(MapToResponse(address));
    }

    public async Task<Result<bool>> DeleteAsync(Guid customerId, Guid addressId)
    {
        var address = await _context.CustomerAddresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.CustomerId == customerId);

        if (address is null)
            return Result<bool>.Failure("Address not found.");

        _context.CustomerAddresses.Remove(address);
        await _context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    private static CustomerAddressResponse MapToResponse(CustomerAddress a) => new(
        a.Id, a.Type, a.Label, a.IsDefault,
        a.Street, a.City, a.State, a.PostalCode, a.Country
    );
}
