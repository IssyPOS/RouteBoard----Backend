using POSShopTicketing.Domain.Common;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Domain.Entities;

/// <summary>One row per status transition - the audit trail behind
/// status tracking, and specifically behind every triage decision
/// (Register/Link/Anonymize/Reject), which is always written here with
/// the acting Manager.</summary>
public class TicketStatusHistory : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }

    public Guid TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    public TicketStatus FromStatus { get; set; }
    public TicketStatus ToStatus { get; set; }

    public Guid? ChangedByTeamMemberId { get; set; }
    public TeamMember? ChangedByTeamMember { get; set; }

    public string? Note { get; set; }

    public DateTime ChangedAt { get; set; }
}
