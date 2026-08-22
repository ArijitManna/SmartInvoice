namespace SmartInvoice.Domain.Entities;

/// <summary>
/// Join table between Identity Roles and Permissions.
/// </summary>
public class RolePermission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string RoleId { get; set; } = string.Empty;
    public Guid PermissionId { get; set; }

    // Navigation
    public Permission Permission { get; set; } = null!;
}
