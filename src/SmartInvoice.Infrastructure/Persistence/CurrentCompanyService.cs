using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SmartInvoice.Infrastructure.Persistence;

public class CurrentCompanyService : ICurrentCompanyService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentCompanyService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? CompanyId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirstValue("CompanyId");
            return !string.IsNullOrEmpty(claim) && Guid.TryParse(claim, out var companyId)
                ? companyId
                : null;
        }
    }

    public Guid? UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return !string.IsNullOrEmpty(claim) && Guid.TryParse(claim, out var userId)
                ? userId
                : null;
        }
    }

    public string? UserName => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
}
