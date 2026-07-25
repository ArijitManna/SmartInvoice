using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        return CreatedAtAction(nameof(GetCurrent), new
        {
            Company = result.Value,
            Message = "Company created. Please re-login to get updated token with CompanyId."
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
