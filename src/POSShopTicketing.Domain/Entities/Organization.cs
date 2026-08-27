using POSShopTicketing.Domain.Common;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Domain.Entities;

/// <summary>
/// The spec's "Client Organization" - a company being supported. Exists
/// purely as attribution and routing data: no login, no portal, no app.
/// Every interaction from this org's side is an email.
/// </summary>
public class Organization : BaseAuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Domain { get; set; } = string.Empty;

    public OrganizationStatus Status { get; set; } = OrganizationStatus.Active;

    public ICollection<OrganizationDepartment> Departments { get; set; } = new List<OrganizationDepartment>();
    public ICollection<OrganizationContact> Contacts { get; set; } = new List<OrganizationContact>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
