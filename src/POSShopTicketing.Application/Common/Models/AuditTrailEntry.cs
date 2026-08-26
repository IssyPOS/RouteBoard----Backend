namespace POSShopTicketing.Application.Common.Models;

/// <summary>
/// One "who did what" event: a login, a logout, or an action on a
/// ticket. IAuditTrailService both persists this (AuditLog table) and,
/// if outbound email is configured, emails it to every active
/// Owner/Admin on the tenant - "the email should show who logged in and
/// out and performs other operations".
/// </summary>
public record AuditTrailEntry
{
    /// <summary>Null only for the rare cross-tenant/system case (there
    /// isn't one here today, but AuditLog itself allows it) - every
    /// event this app currently raises has a real tenant.</summary>
    public Guid? TenantId { get; init; }

    /// <summary>Null when the actor isn't a TeamMember at all - an
    /// inbound email from a customer, or the SLA sweep background job.
    /// ActorDisplayName still describes who/what it was in that case.</summary>
    public Guid? ActorTeamMemberId { get; init; }

    public string ActorDisplayName { get; init; } = string.Empty;

    /// <summary>Machine-readable action code, e.g. "TeamMember.LoggedIn",
    /// "Ticket.Created", "Ticket.StatusChanged".</summary>
    public string Action { get; init; } = string.Empty;

    public string EntityType { get; init; } = string.Empty;
    public string EntityId { get; init; } = string.Empty;

    /// <summary>Human-readable line - what actually shows up in the
    /// audit email and the AuditLog.Details column.</summary>
    public string Summary { get; init; } = string.Empty;
}
