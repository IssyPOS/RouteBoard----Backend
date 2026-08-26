namespace POSShopTicketing.Application.Common.Interfaces;

/// <summary>
/// Backs the spec's zero-setup mailbox option: "a system-provided
/// address ({tenant-slug}@support.routeboard.app, zero setup)". The
/// platform operator configures MX/inbound-parse for one domain, once
/// (see appsettings.json "InboundEmail:SystemDomain" and the README) -
/// after that, every tenant's system-provided address just works
/// immediately, with no DNS action of their own.
/// </summary>
public interface ISystemMailboxAddressProvider
{
    /// <summary>Builds "{tenantSlug}@{configured system domain}". Also
    /// used to tell whether a given address IS the system domain, so a
    /// custom-domain Mailbox can never collide with one.</summary>
    string BuildAddress(string tenantSlug);

    bool IsSystemDomain(string emailAddress);
}
