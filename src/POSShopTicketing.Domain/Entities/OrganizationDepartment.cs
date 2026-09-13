using POSShopTicketing.Domain.Common;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Domain.Entities;

/// <summary>A grouping inside a Client Organization - IT, Sales,
/// Marketing - so tickets can be attributed to a department.</summary>
public class OrganizationDepartment : BaseAuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }

    public Guid OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    public string Name { get; set; } = string.Empty;

    public OrganizationDepartmentStatus Status { get; set; } = OrganizationDepartmentStatus.Active;

    public ICollection<OrganizationContact> Contacts { get; set; } = new List<OrganizationContact>();
}
