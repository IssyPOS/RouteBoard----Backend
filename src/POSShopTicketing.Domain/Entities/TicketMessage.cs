using POSShopTicketing.Domain.Common;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Domain.Entities;

/// <summary>One message in a ticket's thread: an inbound customer email,
/// an outbound agent reply, or an internal note only Team Members can
/// see.</summary>
public class TicketMessage : BaseAuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }

    public Guid TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    public MessageDirection Direction { get; set; }

    public MessageAuthorType AuthorType { get; set; }

    /// <summary>Set when AuthorType is TeamMember (an agent replying or
    /// leaving a note).</summary>
    public Guid? AuthorTeamMemberId { get; set; }
    public TeamMember? AuthorTeamMember { get; set; }

    /// <summary>Set when AuthorType is OrganizationMember - captured
    /// even if the sender wasn't a registered member yet (Unverified
    /// tickets), same reasoning as Ticket.RawSenderEmail.</summary>
    public string? AuthorEmail { get; set; }
    public string? AuthorName { get; set; }

    public string Body { get; set; } = string.Empty;

    /// <summary>The email Message-ID header - either the inbound
    /// message's own id, or the id POSShopTicketing generated when sending an
    /// outbound reply. Used for threading and de-duplication.</summary>
    public string? MessageId { get; set; }

    /// <summary>The Message-ID this one is replying to, mirrored into
    /// the In-Reply-To header on send so the thread stays intact in the
    /// customer's own mail client.</summary>
    public string? InReplyToMessageId { get; set; }

    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}
