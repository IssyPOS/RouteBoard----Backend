namespace POSShopTicketing.Domain.Enums;

/// <summary>
/// Owner/Admin/Manager/Agent are scoped to exactly one Tenant.
/// PlatformSuperAdmin is the SaaS operator - never scoped to a Tenant
/// (TeamMember.TenantId is null for this role) - manages tenants and
/// billing but, by design, cannot see any tenant's ticket content.
/// </summary>
public enum TeamMemberRole
{
    Agent = 0,
    Manager = 1,
    Admin = 2,
    Owner = 3,
    PlatformSuperAdmin = 4
}
