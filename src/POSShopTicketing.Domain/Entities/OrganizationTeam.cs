using POSShopTicketing.Domain.Common;

namespace POSShopTicketing.Domain.Entities;

/// <summary>A grouping inside a Client Organization - IT, Sales,
/// Marketing - so tickets can be attributed to a department.</summary>
public class OrganizationTeam : BaseAuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }

    public Guid OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<OrganizationMember> Members { get; set; } = new List<OrganizationMember>();
}
