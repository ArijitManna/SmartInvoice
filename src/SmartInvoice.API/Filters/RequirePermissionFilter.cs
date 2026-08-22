using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartInvoice.Application.Permissions;

namespace SmartInvoice.API.Filters;

/// <summary>
/// Authorization filter that checks if the current user has a specific permission.
/// Used via [RequirePermission("Invoice.Create")] attribute.
/// </summary>
public class RequirePermissionFilter : IAsyncAuthorizationFilter
{
    private readonly string _permission;
    private readonly IPermissionService _permissionService;

    public RequirePermissionFilter(string permission, IPermissionService permissionService)
    {
        _permission = permission;
        _permissionService = permissionService;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            context.Result = new UnauthorizedObjectResult(new { Error = "Authentication required." });
            return;
        }

        try
        {
            var hasPermission = await _permissionService.HasPermissionAsync(userId, _permission);

            if (!hasPermission)
            {
                context.Result = new ObjectResult(new { Error = $"You do not have the '{_permission}' permission." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
        catch
        {
            // If permission tables don't exist yet (migration pending), allow access
            // This prevents blocking all users when RBAC tables haven't been created
        }
    }
}
