using POSShopTicketing.Domain.Common;

namespace POSShopTicketing.Domain.Entities;

/// <summary>Role/permission and settings changes, tenant-wide. TenantId
/// is nullable to also cover Platform-level actions (e.g. a
/// PlatformSuperAdmin suspending a tenant) that aren't scoped to any one
/// tenant's own audit trail.</summary>
public class AuditLog : BaseEntity
{
    public Guid? TenantId { get; set; }

    public Guid? ActorTeamMemberId { get; set; }

    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;

    public string? Details { get; set; }

    public DateTime CreatedAt { get; set; }
}
