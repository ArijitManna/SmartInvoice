using SmartInvoice.Application.Common;
using SmartInvoice.Application.Companies.DTOs;

namespace SmartInvoice.Application.Companies;

public interface ICompanyService
{
    Task<Result<CompanyResponse>> CreateAsync(CreateCompanyRequest request, string userId);
    Task<Result<CompanyResponse>> GetCurrentAsync();
    Task<Result<CompanyResponse>> UpdateAsync(UpdateCompanyRequest request);
}
