using Microsoft.EntityFrameworkCore;
using SmartInvoice.Application.Common;
using SmartInvoice.Application.Inventory;
using SmartInvoice.Application.Inventory.DTOs;
using SmartInvoice.Domain.Entities;
using SmartInvoice.Infrastructure.Persistence;

namespace SmartInvoice.Infrastructure.Services;

public class WarehouseService : IWarehouseService
{
    private readonly AppDbContext _context;

    public WarehouseService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<WarehouseResponse>> CreateAsync(WarehouseRequest request)
    {
        var warehouse = new Warehouse
        {
            Name = request.Name,
            Code = request.Code,
            Street = request.Street,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country ?? "India",
            ContactPerson = request.ContactPerson,
            Phone = request.Phone,
            IsDefault = request.IsDefault
        };

        if (request.IsDefault)
        {
            var existing = await _context.Warehouses.Where(w => w.IsDefault).ToListAsync();
            foreach (var w in existing) w.IsDefault = false;
        }

        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync();
        return Result<WarehouseResponse>.Success(Map(warehouse));
    }

    public async Task<IReadOnlyList<WarehouseResponse>> GetAllAsync()
    {
        var warehouses = await _context.Warehouses.OrderBy(w => w.Name).ToListAsync();
        return warehouses.Select(Map).ToList().AsReadOnly();
    }

    public async Task<Result<WarehouseResponse>> UpdateAsync(Guid id, WarehouseRequest request)
    {
        var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == id);
        if (warehouse is null) return Result<WarehouseResponse>.Failure("Warehouse not found.");

        warehouse.Name = request.Name;
        warehouse.Code = request.Code;
        warehouse.Street = request.Street;
        warehouse.City = request.City;
        warehouse.State = request.State;
        warehouse.PostalCode = request.PostalCode;
        warehouse.Country = request.Country ?? "India";
        warehouse.ContactPerson = request.ContactPerson;
        warehouse.Phone = request.Phone;
        warehouse.IsDefault = request.IsDefault;

        if (request.IsDefault)
        {
            var existing = await _context.Warehouses.Where(w => w.IsDefault && w.Id != id).ToListAsync();
            foreach (var w in existing) w.IsDefault = false;
        }

        await _context.SaveChangesAsync();
        return Result<WarehouseResponse>.Success(Map(warehouse));
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == id);
        if (warehouse is null) return Result<bool>.Failure("Warehouse not found.");

        warehouse.IsDeleted = true;
        await _context.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    private static WarehouseResponse Map(Warehouse w) => new(
        w.Id, w.Name, w.Code, w.Street, w.City, w.State, w.PostalCode, w.Country, w.ContactPerson, w.Phone, w.IsDefault
    );
}
