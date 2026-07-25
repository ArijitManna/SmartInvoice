namespace SmartInvoice.Application.Auth.DTOs;

public record AuthResponse(
    string UserId,
    string Email,
    string FullName,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    Guid? CompanyId,
    IReadOnlyList<string> Roles
);
