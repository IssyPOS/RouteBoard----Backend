namespace POSShopTicketing.Infrastructure.Services;

/// <summary>
/// Bound from the "InboundEmail" section of appsettings.json. SystemDomain
/// is the ONE domain the platform operator configures MX/inbound-parse
/// for, once - every tenant's zero-setup Mailbox then rides on
/// {tenant.Slug}@{SystemDomain} automatically, with no DNS action of
/// their own. See README "Email ingestion" for the one-time setup.
/// </summary>
public class InboundEmailSettings
{
    public const string SectionName = "InboundEmail";

    public string SystemDomain { get; set; } = string.Empty;
}
