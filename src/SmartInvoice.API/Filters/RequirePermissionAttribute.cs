using Microsoft.AspNetCore.Mvc;

namespace SmartInvoice.API.Filters;

/// <summary>
/// Attribute to declare which permission is required for an action/controller.
/// Works with RequirePermissionFilter to enforce the check.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class RequirePermissionAttribute : TypeFilterAttribute
{
    public RequirePermissionAttribute(string permission)
        : base(typeof(RequirePermissionFilter))
    {
        Arguments = [permission];
    }
}
