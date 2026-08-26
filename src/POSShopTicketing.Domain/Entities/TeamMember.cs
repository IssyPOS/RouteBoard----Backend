using POSShopTicketing.Domain.Common;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Domain.Entities;

/// <summary>
/// A login-bearing user. Owner/Admin/Manager/Agent belong to exactly one
/// Tenant; PlatformSuperAdmin has a null TenantId (the SaaS operator,
/// never scoped to - or able to see the ticket content of - any single
/// tenant). Doubles as the "Agent" tickets get assigned to; no separate
/// Agent table.
/// </summary>
public class TeamMember : BaseAuditableEntity, ITenantScoped
{
    /// <summary>Guid.Empty sentinel for PlatformSuperAdmin (never scoped
    /// to a tenant) - kept non-nullable so TeamMember can implement
    /// ITenantScoped like every other entity, rather than special-casing
    /// the global query filter for just this one table. A real Tenant's
    /// Id is always a random BaseEntity-generated Guid, so it can never
    /// collide with Guid.Empty. ICurrentTenantService.TenantId (Guid?)
    /// maps null -> Guid.Empty at the filter boundary.</summary>
    public Guid TenantId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public TeamMemberRole Role { get; set; } = TeamMemberRole.Agent;

    public TeamMemberStatus Status { get; set; } = TeamMemberStatus.Invited;

    // ---- Invite flow (POST /auth/invite -> POST /auth/invite/accept) ----
    public string? InviteTokenHash { get; set; }
    public DateTime? InviteTokenExpiresAt { get; set; }

    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();
    public ICollection<RefreshTokenz> RefreshTokens { get; set; } = new List<RefreshTokenz>();

    public string FullName => $"{FirstName} {LastName}".Trim();
}
