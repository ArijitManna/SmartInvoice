using Microsoft.EntityFrameworkCore;
using SmartInvoice.Application.Common;
using SmartInvoice.Application.Customers;
using SmartInvoice.Application.Customers.DTOs;
using SmartInvoice.Domain.Entities;
using SmartInvoice.Domain.ValueObjects;
using SmartInvoice.Infrastructure.Persistence;

namespace SmartInvoice.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _context;
    private readonly ICurrentCompanyService _companyService;

    public CustomerService(AppDbContext context, ICurrentCompanyService companyService)
    {
        _context = context;
        _companyService = companyService;
    }

    public async Task<Result<CustomerResponse>> CreateAsync(CreateCustomerRequest request)
    {
        var customer = new Customer
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            ContactPerson = request.ContactPerson,
            Notes = request.Notes,
            GstInfo = new GstInfo
            {
                Gstin = request.Gstin,
                Pan = request.Pan,
                StateCode = request.GstStateCode
            },
            BillingAddress = new Address
            {
                Street = request.BillingStreet ?? string.Empty,
                City = request.BillingCity ?? string.Empty,
                State = request.BillingState ?? string.Empty,
                PostalCode = request.BillingPostalCode ?? string.Empty,
                Country = request.BillingCountry ?? "India"
            },
            ShippingAddress = HasShippingAddress(request) ? new Address
            {
                Street = request.ShippingStreet ?? string.Empty,
                City = request.ShippingCity ?? string.Empty,
                State = request.ShippingState ?? string.Empty,
                PostalCode = request.ShippingPostalCode ?? string.Empty,
                Country = request.ShippingCountry ?? "India"
            } : null
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return Result<CustomerResponse>.Success(MapToResponse(customer));
    }

    public async Task<Result<CustomerResponse>> GetByIdAsync(Guid id)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);

        if (customer is null)
        {
            return Result<CustomerResponse>.Failure("Customer not found.");
        }

        return Result<CustomerResponse>.Success(MapToResponse(customer));
    }

    public async Task<PagedResult<CustomerResponse>> GetAllAsync(
        int page = 1, int pageSize = 20, string? search = null, string? sortBy = null, bool sortDesc = false)
    {
        var query = _context.Customers
            .Where(c => c.CompanyId == _companyService.CompanyId)
            .AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(search))
        {
            string searchLower = search.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(searchLower) ||
                (c.Email != null && c.Email.ToLower().Contains(searchLower)) ||
                (c.Phone != null && c.Phone.Contains(search)) ||
                (c.ContactPerson != null && c.ContactPerson.ToLower().Contains(searchLower)));
        }

        // Sort
        query = sortBy?.ToLower() switch
        {
            "name" => sortDesc ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "email" => sortDesc ? query.OrderByDescending(c => c.Email) : query.OrderBy(c => c.Email),
            "createdat" => sortDesc ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<CustomerResponse>(
            Items: items.Select(MapToResponse).ToList().AsReadOnly(),
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize
        );
    }

    public async Task<Result<CustomerResponse>> UpdateAsync(Guid id, UpdateCustomerRequest request)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);

        if (customer is null)
        {
            return Result<CustomerResponse>.Failure("Customer not found.");
        }

        customer.Name = request.Name;
        customer.Email = request.Email;
        customer.Phone = request.Phone;
        customer.ContactPerson = request.ContactPerson;
        customer.Notes = request.Notes;

        customer.GstInfo = new GstInfo
        {
            Gstin = request.Gstin,
            Pan = request.Pan,
            StateCode = request.GstStateCode
        };

        customer.BillingAddress = new Address
        {
            Street = request.BillingStreet ?? string.Empty,
            City = request.BillingCity ?? string.Empty,
            State = request.BillingState ?? string.Empty,
            PostalCode = request.BillingPostalCode ?? string.Empty,
            Country = request.BillingCountry ?? "India"
        };

        customer.ShippingAddress = HasShippingAddress(request) ? new Address
        {
            Street = request.ShippingStreet ?? string.Empty,
            City = request.ShippingCity ?? string.Empty,
            State = request.ShippingState ?? string.Empty,
            PostalCode = request.ShippingPostalCode ?? string.Empty,
            Country = request.ShippingCountry ?? "India"
        } : null;

        await _context.SaveChangesAsync();

        return Result<CustomerResponse>.Success(MapToResponse(customer));
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);

        if (customer is null)
        {
            return Result<bool>.Failure("Customer not found.");
        }

        customer.IsDeleted = true;
        await _context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    private static bool HasShippingAddress(CreateCustomerRequest r) =>
        !string.IsNullOrWhiteSpace(r.ShippingStreet) ||
        !string.IsNullOrWhiteSpace(r.ShippingCity) ||
        !string.IsNullOrWhiteSpace(r.ShippingState);

    private static bool HasShippingAddress(UpdateCustomerRequest r) =>
        !string.IsNullOrWhiteSpace(r.ShippingStreet) ||
        !string.IsNullOrWhiteSpace(r.ShippingCity) ||
        !string.IsNullOrWhiteSpace(r.ShippingState);

    private static CustomerResponse MapToResponse(Customer c)
    {
        return new CustomerResponse(
            Id: c.Id,
            Name: c.Name,
            Email: c.Email,
            Phone: c.Phone,
            ContactPerson: c.ContactPerson,
            Notes: c.Notes,
            Gstin: c.GstInfo.Gstin,
            Pan: c.GstInfo.Pan,
            GstStateCode: c.GstInfo.StateCode,
            BillingStreet: c.BillingAddress.Street,
            BillingCity: c.BillingAddress.City,
            BillingState: c.BillingAddress.State,
            BillingPostalCode: c.BillingAddress.PostalCode,
            BillingCountry: c.BillingAddress.Country,
            ShippingStreet: c.ShippingAddress?.Street,
            ShippingCity: c.ShippingAddress?.City,
            ShippingState: c.ShippingAddress?.State,
            ShippingPostalCode: c.ShippingAddress?.PostalCode,
            ShippingCountry: c.ShippingAddress?.Country,
            CreatedAt: c.CreatedAt
        );
    }
}
