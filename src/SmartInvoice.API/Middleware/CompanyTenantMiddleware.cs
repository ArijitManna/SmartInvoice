using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using SmartInvoice.Infrastructure.Persistence;

namespace SmartInvoice.API.Middleware;

/// <summary>
/// Enforces multi-tenancy by requiring CompanyId in JWT for all protected endpoints.
/// Users must complete company onboarding before accessing data operations.
/// Returns 403 Forbidden if user has no CompanyId assigned.
/// </summary>
public class CompanyTenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CompanyTenantMiddleware> _logger;

    // Routes that bypass CompanyId check (auth, onboarding, health checks)
    private static readonly HashSet<string> ExemptPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/auth",
        "/auth/login",
        "/auth/register",
        "/auth/refresh-token",
        "/auth/logout",
        "/onboarding",
        "/onboarding/create-company",
        "/companies/create",
        "/health",
        "/health/live",
        "/health/ready",
        "/hangfire",
        "/",
        "/swagger",
        "/openapi"
    };

    public CompanyTenantMiddleware(RequestDelegate next, ILogger<CompanyTenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentCompanyService companyService)
    {
        var path = context.Request.Path.Value ?? "/";
        var method = context.Request.Method;

        // Skip exempt paths
        if (IsExemptPath(path))
        {
            await _next(context);
            return;
        }

        // Check if user is authenticated
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        // Check if user has CompanyId in JWT
        if (companyService.CompanyId == null)
        {
            _logger.LogWarning(
                "User {UserId} ({UserName}) attempted to access {Path} {Method} without CompanyId assignment.",
                companyService.UserId, companyService.UserName, path, method);

            await ReturnForbiddenResponse(context);
            return;
        }

        await _next(context);
    }

    private static bool IsExemptPath(string path)
    {
        return ExemptPaths.Any(exempt =>
            path.StartsWith(exempt, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task ReturnForbiddenResponse(HttpContext context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        context.Response.ContentType = "application/problem+json";

        var problem = new
        {
            type = "https://httpstatuses.com/403",
            title = "Forbidden",
            status = 403,
            detail = "Your account is not assigned to a company. Please complete company onboarding to proceed.",
            traceId = context.TraceIdentifier
        };

        var json = JsonSerializer.Serialize(problem,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}
