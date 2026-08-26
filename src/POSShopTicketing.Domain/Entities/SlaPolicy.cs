using POSShopTicketing.Domain.Common;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Domain.Entities;

/// <summary>Phase 5 in the roadmap, but already implemented here:
/// configurable first-response/resolution targets per Priority, per
/// Tenant. Feature-flaggable per tenant via Tenant.Plan if you want SLA
/// tracking to be a paid-tier feature later.</summary>
public class SlaPolicy : BaseAuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }

    public TicketPriority Priority { get; set; }

    public int FirstResponseTargetMinutes { get; set; }
    public int ResolutionTargetMinutes { get; set; }
}
