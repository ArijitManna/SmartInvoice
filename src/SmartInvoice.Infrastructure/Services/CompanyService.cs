using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartInvoice.Application.Common;
using SmartInvoice.Application.Companies;
using SmartInvoice.Application.Companies.DTOs;
using SmartInvoice.Domain.Entities;
using SmartInvoice.Domain.ValueObjects;
using SmartInvoice.Infrastructure.Identity;
using SmartInvoice.Infrastructure.Persistence;

namespace SmartInvoice.Infrastructure.Services;

public class CompanyService : ICompanyService
{
    private readonly AppDbContext _context;
    private readonly ICurrentCompanyService _currentCompanyService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CompanyService(
        AppDbContext context,
        ICurrentCompanyService currentCompanyService,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _currentCompanyService = currentCompanyService;
        _userManager = userManager;
    }

    public async Task<Result<CompanyResponse>> CreateAsync(CreateCompanyRequest request, string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Result<CompanyResponse>.Failure("User not found.");
        }

        if (user.CompanyId.HasValue)
        {
            return Result<CompanyResponse>.Failure("User already has a company assigned.");
        }

        var company = new Company
        {
            Name = request.Name,
            Phone = request.Phone,
            Email = request.Email,
            Website = request.Website,
            DefaultCurrency = request.DefaultCurrency ?? "INR",
            TimeZone = request.TimeZone ?? "Asia/Kolkata",
            InvoicePrefix = request.InvoicePrefix ?? "INV",
            Address = new Address
            {
                Street = request.Street ?? string.Empty,
                City = request.City ?? string.Empty,
                State = request.State ?? string.Empty,
                PostalCode = request.PostalCode ?? string.Empty,
                Country = request.Country ?? "India"
            },
            GstInfo = new GstInfo
            {
                Gstin = request.Gstin,
                Pan = request.Pan,
                StateCode = request.GstStateCode
            },
            BankDetails = new BankDetails
            {
                BankName = request.BankName ?? string.Empty,
                AccountNumber = request.AccountNumber ?? string.Empty,
                IfscCode = request.IfscCode ?? string.Empty,
                AccountHolderName = request.AccountHolderName ?? string.Empty,
                BranchName = request.BranchName,
                UpiId = request.UpiId
            }
        };

        // Self-reference: CompanyId = own Id
        company.CompanyId = company.Id;

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

        // Link user to company
        user.CompanyId = company.Id;
        await _userManager.UpdateAsync(user);

        return Result<CompanyResponse>.Success(MapToResponse(company));
    }

    public async Task<Result<CompanyResponse>> GetCurrentAsync()
    {
        var companyId = _currentCompanyService.CompanyId;
        if (!companyId.HasValue)
        {
            return Result<CompanyResponse>.Failure("No company associated with current user.");
        }

        var company = await _context.Companies
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == companyId.Value && !c.IsDeleted);

        if (company is null)
        {
            return Result<CompanyResponse>.Failure("Company not found.");
        }

        return Result<CompanyResponse>.Success(MapToResponse(company));
    }

    public async Task<Result<CompanyResponse>> UpdateAsync(UpdateCompanyRequest request)
    {
        var companyId = _currentCompanyService.CompanyId;
        if (!companyId.HasValue)
        {
            return Result<CompanyResponse>.Failure("No company associated with current user.");
        }

        var company = await _context.Companies
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == companyId.Value && !c.IsDeleted);

        if (company is null)
        {
            return Result<CompanyResponse>.Failure("Company not found.");
        }

        company.Name = request.Name;
        company.Phone = request.Phone;
        company.Email = request.Email;
        company.Website = request.Website;
        company.LogoUrl = request.LogoUrl;
        company.SignatureUrl = request.SignatureUrl;
        company.DefaultCurrency = request.DefaultCurrency ?? "INR";
        company.TimeZone = request.TimeZone ?? "Asia/Kolkata";
        company.InvoicePrefix = request.InvoicePrefix ?? "INV";

        company.Address = new Address
        {
            Street = request.Street ?? string.Empty,
            City = request.City ?? string.Empty,
            State = request.State ?? string.Empty,
            PostalCode = request.PostalCode ?? string.Empty,
            Country = request.Country ?? "India"
        };

        company.GstInfo = new GstInfo
        {
            Gstin = request.Gstin,
            Pan = request.Pan,
            StateCode = request.GstStateCode
        };

        company.BankDetails = new BankDetails
        {
            BankName = request.BankName ?? string.Empty,
            AccountNumber = request.AccountNumber ?? string.Empty,
            IfscCode = request.IfscCode ?? string.Empty,
            AccountHolderName = request.AccountHolderName ?? string.Empty,
            BranchName = request.BranchName,
            UpiId = request.UpiId
        };

        await _context.SaveChangesAsync();

        return Result<CompanyResponse>.Success(MapToResponse(company));
    }

    private static CompanyResponse MapToResponse(Company company)
    {
        return new CompanyResponse(
            Id: company.Id,
            Name: company.Name,
            Phone: company.Phone,
            Email: company.Email,
            Website: company.Website,
            LogoUrl: company.LogoUrl,
            SignatureUrl: company.SignatureUrl,
            DefaultCurrency: company.DefaultCurrency,
            TimeZone: company.TimeZone,
            InvoicePrefix: company.InvoicePrefix,
            NextInvoiceNumber: company.NextInvoiceNumber,
            Street: company.Address.Street,
            City: company.Address.City,
            State: company.Address.State,
            PostalCode: company.Address.PostalCode,
            Country: company.Address.Country,
            Gstin: company.GstInfo.Gstin,
            Pan: company.GstInfo.Pan,
            GstStateCode: company.GstInfo.StateCode,
            BankName: company.BankDetails.BankName,
            AccountNumber: company.BankDetails.AccountNumber,
            IfscCode: company.BankDetails.IfscCode,
            AccountHolderName: company.BankDetails.AccountHolderName,
            BranchName: company.BankDetails.BranchName,
            UpiId: company.BankDetails.UpiId
        );
    }
}
