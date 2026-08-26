namespace POSShopTicketing.Domain.Entities;

/// <summary>Many-to-many join between Ticket and Tag. Composite key
/// (TicketId, TagId) - configured in TicketTagConfiguration.</summary>
public class TicketTag
{
    public Guid TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    public Guid TagId { get; set; }
    public Tag? Tag { get; set; }
}
