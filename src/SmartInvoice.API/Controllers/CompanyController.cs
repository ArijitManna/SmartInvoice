using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInvoice.API.Filters;
using SmartInvoice.Application.Auth;
using SmartInvoice.Application.Auth.DTOs;
using SmartInvoice.Application.Companies;
using SmartInvoice.Application.Companies.DTOs;

namespace SmartInvoice.API.Controllers;

[ApiController]
[Route("api/companies")]
[Authorize]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly IAuthService _authService;

    public CompanyController(ICompanyService companyService, IAuthService authService)
    {
        _companyService = companyService;
        _authService = authService;
    }

    /// <summary>
    /// Creates a company and links it to the current user.
    /// Returns a new auth token with the CompanyId claim.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCompanyRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _companyService.CreateAsync(request, userId);

        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error });
        }

        // Issue a fresh token with CompanyId claim
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        var tokenResult = await _authService.LoginAsync(new LoginRequest(email, string.Empty));

        if (!tokenResult.IsSuccess)
        {
            return Ok(new
            {
                Company = result.Value,
                Message = "Company created. Token refresh failed, please re-login.",
                Error = tokenResult.Error
            });
        }

        return Ok(new
        {
            Company = result.Value,
            AccessToken = tokenResult.Value!.AccessToken,
            RefreshToken = tokenResult.Value!.RefreshToken,
            UserId = tokenResult.Value!.UserId,
            Email = tokenResult.Value!.Email,
            FullName = tokenResult.Value!.FullName,
            CompanyId = tokenResult.Value!.CompanyId,
            Roles = tokenResult.Value!.Roles,
            Message = "Company created successfully with updated token."
        });
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent()
    {
        var result = await _companyService.GetCurrentAsync();

        if (!result.IsSuccess)
        {
            return NotFound(new { Error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPut("current")]
    [RequirePermission("Settings.Manage")]
    public async Task<IActionResult> Update([FromBody] UpdateCompanyRequest request)
    {
        var result = await _companyService.UpdateAsync(request);

        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error });
        }

        return Ok(result.Value);
    }
}
