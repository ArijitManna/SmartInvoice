namespace SmartInvoice.Infrastructure.Persistence;

/// <summary>
/// Provides the current tenant (company) context for multi-tenancy query filtering.
/// Implemented in the API layer to extract CompanyId from JWT claims.
/// </summary>
public interface ICurrentCompanyService
{
    Guid? CompanyId { get; }
    Guid? UserId { get; }
    string? UserName { get; }
}
