namespace POSShopTicketing.Infrastructure.Services;

/// <summary>Bound from the "AuditEmail" section of appsettings.json.
/// AuditLog rows are always written regardless of this setting - Enabled
/// only controls whether IAuditTrailService also emails the tenant's
/// Owners/Admins for every event.</summary>
public class AuditEmailSettings
{
    public const string SectionName = "AuditEmail";

    public bool Enabled { get; set; } = true;
}
