namespace POSShopTicketing.Application.Common.Interfaces;

/// <summary>
/// The current request's tenant, read from the "tenant_id" JWT claim.
/// Null for a PlatformSuperAdmin (never scoped to a tenant) or for the
/// public endpoints that run before a tenant is known (login, tenant
/// registration, invite acceptance).
/// ApplicationDbContext uses this for its global query filters, and
/// TenantSessionInterceptor uses it to set the app.current_tenant_id
/// Postgres session variable Row-Level Security policies key off.
/// </summary>
public interface ICurrentTenantService
{
    Guid? TenantId { get; }

    /// <summary>
    /// Explicit override for the two flows that resolve which tenant
    /// they're operating on mid-request, from a source other than the
    /// JWT: IngestInboundEmailCommand (resolves it from the target
    /// Mailbox) and RegisterTenantCommand (creates the tenant itself).
    /// Both call this immediately once the tenant is known, so
    /// TenantSessionInterceptor picks up the right value for every
    /// subsequent query in the same request - without it, Postgres RLS
    /// would still be scoped to "no tenant" and reject the writes these
    /// two flows need to make.
    /// </summary>
    void SetTenant(Guid tenantId);
}
