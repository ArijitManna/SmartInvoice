using Microsoft.EntityFrameworkCore;
using SmartInvoice.Application.Common;
using SmartInvoice.Application.Purchase;
using SmartInvoice.Application.Purchase.DTOs;
using SmartInvoice.Domain.Entities;
using SmartInvoice.Domain.ValueObjects;
using SmartInvoice.Infrastructure.Persistence;

namespace SmartInvoice.Infrastructure.Services;

public class VendorService : IVendorService
{
    private readonly AppDbContext _context;
    private readonly ICurrentCompanyService _companyService;

    public VendorService(AppDbContext context, ICurrentCompanyService companyService)
    {
        _context = context;
        _companyService = companyService;
    }

    public async Task<Result<VendorResponse>> CreateAsync(VendorRequest request)
    {
        var vendor = new Vendor
        {
            Name = request.Name, Email = request.Email, Phone = request.Phone,
            ContactPerson = request.ContactPerson, Notes = request.Notes,
            GstInfo = new GstInfo { Gstin = request.Gstin, Pan = request.Pan, StateCode = request.GstStateCode },
            Address = new Address { Street = request.Street ?? "", City = request.City ?? "", State = request.State ?? "", PostalCode = request.PostalCode ?? "", Country = request.Country ?? "India" },
            BankDetails = new BankDetails { BankName = request.BankName ?? "", AccountNumber = request.AccountNumber ?? "", IfscCode = request.IfscCode ?? "", AccountHolderName = request.AccountHolderName ?? "", BranchName = request.BranchName, UpiId = request.UpiId }
        };
        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync();
        return Result<VendorResponse>.Success(Map(vendor));
    }

    public async Task<Result<VendorResponse>> GetByIdAsync(Guid id)
    {
        var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Id == id);
        return vendor is null ? Result<VendorResponse>.Failure("Vendor not found.") : Result<VendorResponse>.Success(Map(vendor));
    }

    public async Task<PagedResult<VendorResponse>> GetAllAsync(int page = 1, int pageSize = 20, string? search = null)
    {
        var query = _context.Vendors
            .Where(v => v.CompanyId == _companyService.CompanyId)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(v => v.Name.ToLower().Contains(s) || (v.Email != null && v.Email.ToLower().Contains(s)));
        }
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(v => v.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedResult<VendorResponse>(items.Select(Map).ToList().AsReadOnly(), total, page, pageSize);
    }

    public async Task<Result<VendorResponse>> UpdateAsync(Guid id, VendorRequest request)
    {
        var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Id == id);
        if (vendor is null) return Result<VendorResponse>.Failure("Vendor not found.");
        vendor.Name = request.Name; vendor.Email = request.Email; vendor.Phone = request.Phone;
        vendor.ContactPerson = request.ContactPerson; vendor.Notes = request.Notes;
        vendor.GstInfo = new GstInfo { Gstin = request.Gstin, Pan = request.Pan, StateCode = request.GstStateCode };
        vendor.Address = new Address { Street = request.Street ?? "", City = request.City ?? "", State = request.State ?? "", PostalCode = request.PostalCode ?? "", Country = request.Country ?? "India" };
        vendor.BankDetails = new BankDetails { BankName = request.BankName ?? "", AccountNumber = request.AccountNumber ?? "", IfscCode = request.IfscCode ?? "", AccountHolderName = request.AccountHolderName ?? "", BranchName = request.BranchName, UpiId = request.UpiId };
        await _context.SaveChangesAsync();
        return Result<VendorResponse>.Success(Map(vendor));
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Id == id);
        if (vendor is null) return Result<bool>.Failure("Vendor not found.");
        vendor.IsDeleted = true;
        await _context.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    private static VendorResponse Map(Vendor v) => new(v.Id, v.Name, v.Email, v.Phone, v.ContactPerson, v.Notes, v.OutstandingBalance, v.GstInfo.Gstin, v.GstInfo.Pan, v.Address.City, v.Address.State, v.CreatedAt);
}
