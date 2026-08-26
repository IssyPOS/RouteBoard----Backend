using POSShopTicketing.Domain.Common;

namespace POSShopTicketing.Domain.Entities;

public class Tag : BaseAuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<TicketTag> Tickets { get; set; } = new List<TicketTag>();
}
