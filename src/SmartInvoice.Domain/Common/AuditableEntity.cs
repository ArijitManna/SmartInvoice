namespace SmartInvoice.Domain.Common;

/// <summary>
/// Extends BaseEntity with soft-delete timestamp tracking.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public void MarkAsDeleted(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }
}
