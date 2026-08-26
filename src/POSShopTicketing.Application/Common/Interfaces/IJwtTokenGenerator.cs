using POSShopTicketing.Domain.Entities;

namespace POSShopTicketing.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    /// <summary>Issues a signed, short-lived JWT access token for the
    /// given TeamMember. TenantId is null for a PlatformSuperAdmin, so
    /// the "tenant_id" claim is simply omitted for them.</summary>
    (string Token, DateTime ExpiresAt) GenerateAccessToken(TeamMember teamMember);
}
