using POSShopTicketing.Domain.Common;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Domain.Entities;

/// <summary>A person at the client who emails in - identified by email
/// address alone, never by a login.</summary>
public class OrganizationMember : BaseAuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }

    public Guid OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    /// <summary>Optional - a member can exist directly under an
    /// Organization with no department grouping yet.</summary>
    public Guid? OrganizationTeamId { get; set; }
    public OrganizationTeam? OrganizationTeam { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public OrganizationMemberStatus Status { get; set; } = OrganizationMemberStatus.Active;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
