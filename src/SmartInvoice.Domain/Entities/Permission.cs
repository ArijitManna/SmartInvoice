namespace SmartInvoice.Domain.Entities;

/// <summary>
/// Represents a granular permission (e.g., "Invoice.Create", "Report.View").
/// Not tenant-scoped — permissions are global.
/// </summary>
public class Permission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
