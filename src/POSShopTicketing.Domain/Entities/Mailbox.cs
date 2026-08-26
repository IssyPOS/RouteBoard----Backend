using POSShopTicketing.Domain.Common;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Domain.Entities;

/// <summary>One row per inbound address a Tenant receives support email
/// at - either the system-provided {slug}@... address (zero setup) or
/// the Tenant's own domain forwarded to this app's webhook.</summary>
public class Mailbox : BaseAuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }

    public string EmailAddress { get; set; } = string.Empty;

    public MailboxProvider Provider { get; set; } = MailboxProvider.SendGridInboundParse;

    /// <summary>Shared secret used to verify the inbound-parse webhook
    /// really came from the configured provider.</summary>
    public string WebhookSecret { get; set; } = string.Empty;

    public string? Signature { get; set; }

    public bool IsVerified { get; set; }

    /// <summary>True for the zero-setup {tenant-slug}@{platform inbound
    /// domain} address (see ISystemMailboxAddressProvider) - false for a
    /// Tenant's own bring-your-own domain, which still needs a real DNS/
    /// forwarding change and a POST .../verify call before use.</summary>
    public bool IsSystemProvided { get; set; }

    public bool IsDefault { get; set; }

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
