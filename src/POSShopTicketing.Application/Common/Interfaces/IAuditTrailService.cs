using POSShopTicketing.Application.Common.Models;

namespace POSShopTicketing.Application.Common.Interfaces;

/// <summary>
/// Records an AuditTrailEntry to the AuditLog table (always) and, if
/// outbound email is configured and enabled, emails it to every active
/// Owner/Admin on the tenant (see Infrastructure/Services/AuditTrailService
/// and the "AuditEmail" appsettings section).
/// </summary>
public interface IAuditTrailService
{
    Task RecordAsync(AuditTrailEntry entry, CancellationToken cancellationToken);
}
