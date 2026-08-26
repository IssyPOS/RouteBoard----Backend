using POSShopTicketing.Domain.Common;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Domain.Entities;

/// <summary>
/// The spec's "Assignment Preset": this Organization / OrganizationTeam
/// / OrganizationMember always routes to this TeamMember.
/// TicketAssignmentService tries rules most-specific first (Member, then
/// OrganizationTeam, then Organization), using PriorityOrder to break
/// ties when more than one rule matches the same scope level.
/// </summary>
public class AssignmentRule : BaseAuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }

    public AssignmentScopeType ScopeType { get; set; }

    /// <summary>Id of the Organization, OrganizationTeam, or
    /// OrganizationMember this rule applies to, per ScopeType.</summary>
    public Guid ScopeId { get; set; }

    public Guid AssignedToTeamMemberId { get; set; }
    public TeamMember? AssignedToTeamMember { get; set; }

    public int PriorityOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
