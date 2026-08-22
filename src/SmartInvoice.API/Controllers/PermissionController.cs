using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInvoice.API.Filters;
using SmartInvoice.Application.Permissions;
using SmartInvoice.Domain.Entities;
using SmartInvoice.Infrastructure.Persistence;

namespace SmartInvoice.API.Controllers;

[ApiController]
[Route("api/permissions")]
[Authorize]
public class PermissionController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IPermissionService _permissionService;

    public PermissionController(AppDbContext context, RoleManager<IdentityRole> roleManager, IPermissionService permissionService)
    {
        _context = context;
        _roleManager = roleManager;
        _permissionService = permissionService;
    }

    /// <summary>
    /// Get all available permissions grouped by module.
    /// </summary>
    [HttpGet]
    [RequirePermission("User.Manage")]
    public async Task<IActionResult> GetAllPermissions()
    {
        var permissions = await _context.Permissions
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Action)
            .ToListAsync();

        var grouped = permissions
            .GroupBy(p => p.Module)
            .Select(g => new
            {
                Module = g.Key,
                Permissions = g.Select(p => new { p.Id, p.Name, p.Action, p.Description })
            });

        return Ok(grouped);
    }

    /// <summary>
    /// Get all roles with their assigned permissions.
    /// </summary>
    [HttpGet("roles")]
    [RequirePermission("User.Manage")]
    public async Task<IActionResult> GetRolesWithPermissions()
    {
        var roles = await _roleManager.Roles.ToListAsync();

        var result = new List<object>();
        foreach (var role in roles)
        {
            var permNames = await _permissionService.GetRolePermissionsAsync(role.Id);
            result.Add(new
            {
                role.Id,
                role.Name,
                Permissions = permNames
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Get permissions for a specific role.
    /// </summary>
    [HttpGet("roles/{roleId}")]
    [RequirePermission("User.Manage")]
    public async Task<IActionResult> GetRolePermissions(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role is null)
            return NotFound(new { Error = "Role not found." });

        var permNames = await _permissionService.GetRolePermissionsAsync(roleId);

        return Ok(new { role.Id, role.Name, Permissions = permNames });
    }

    /// <summary>
    /// Update permissions for a role (replace all).
    /// </summary>
    [HttpPut("roles/{roleId}")]
    [RequirePermission("User.Manage")]
    public async Task<IActionResult> UpdateRolePermissions(string roleId, [FromBody] UpdateRolePermissionsRequest request)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role is null)
            return NotFound(new { Error = "Role not found." });

        // Validate permission IDs
        var validPermissionIds = await _context.Permissions
            .Where(p => request.PermissionIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync();

        // Remove existing role permissions
        var existing = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();
        _context.RolePermissions.RemoveRange(existing);

        // Add new ones
        var newMappings = validPermissionIds.Select(pid => new RolePermission
        {
            RoleId = roleId,
            PermissionId = pid
        });
        _context.RolePermissions.AddRange(newMappings);

        await _context.SaveChangesAsync();

        // Invalidate cache (service uses 5-min TTL so it'll refresh eventually,
        // but for immediate effect we can't easily clear IMemoryCache entries per key here)

        var updatedPerms = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission.Name)
            .ToListAsync();

        return Ok(new { role.Id, role.Name, Permissions = updatedPerms });
    }

    /// <summary>
    /// Get current user's permissions.
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyPermissions()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized();

        var permissions = await _permissionService.GetUserPermissionsAsync(userId);

        return Ok(new { Permissions = permissions });
    }
}

public record UpdateRolePermissionsRequest(List<Guid> PermissionIds);
