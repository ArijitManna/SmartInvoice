namespace SmartInvoice.Application.Permissions;

public interface IPermissionService
{
    /// <summary>
    /// Checks if a user has a specific permission based on their roles.
    /// </summary>
    Task<bool> HasPermissionAsync(string userId, string permissionName);

    /// <summary>
    /// Gets all permissions assigned to a user (via their roles).
    /// </summary>
    Task<IReadOnlyList<string>> GetUserPermissionsAsync(string userId);

    /// <summary>
    /// Gets all permissions assigned to a specific role.
    /// </summary>
    Task<IReadOnlyList<string>> GetRolePermissionsAsync(string roleId);
}
