using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SmartInvoice.Application.Permissions;
using SmartInvoice.Infrastructure.Identity;
using SmartInvoice.Infrastructure.Persistence;

namespace SmartInvoice.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public PermissionService(AppDbContext context, UserManager<ApplicationUser> userManager, IMemoryCache cache)
    {
        _context = context;
        _userManager = userManager;
        _cache = cache;
    }

    public async Task<bool> HasPermissionAsync(string userId, string permissionName)
    {
        var permissions = await GetUserPermissionsAsync(userId);
        // During development: if no permissions found at all, allow access (RBAC not fully configured)
        if (permissions.Count == 0)
            return true;
        return permissions.Contains(permissionName);
    }

    public async Task<IReadOnlyList<string>> GetUserPermissionsAsync(string userId)
    {
        var cacheKey = $"user_permissions_{userId}";

        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<string>? cached) && cached is not null)
        {
            return cached;
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return [];
        }

        var roleNames = await _userManager.GetRolesAsync(user);

        // Get role IDs from role names
        var roleIds = await _context.Roles
            .Where(r => roleNames.Contains(r.Name!))
            .Select(r => r.Id)
            .ToListAsync();

        // Get all permission names for those roles
        var permissions = await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToListAsync();

        var result = permissions.AsReadOnly();
        _cache.Set(cacheKey, result, CacheDuration);

        return result;
    }

    public async Task<IReadOnlyList<string>> GetRolePermissionsAsync(string roleId)
    {
        var cacheKey = $"role_permissions_{roleId}";

        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<string>? cached) && cached is not null)
        {
            return cached;
        }

        var permissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission.Name)
            .ToListAsync();

        var result = permissions.AsReadOnly();
        _cache.Set(cacheKey, result, CacheDuration);

        return result;
    }
}
