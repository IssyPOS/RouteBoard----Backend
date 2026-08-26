using System.Data.Common;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Diagnostics;
using POSShopTicketing.Application.Common.Interfaces;

namespace POSShopTicketing.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Isolation, layer 2: keeps the app.current_tenant_id Postgres session
/// variable (which the Row-Level Security policies in
/// Persistence/Scripts/enable-row-level-security.sql key off) in sync
/// with ICurrentTenantService before every command executes - not just
/// once at connection-open, because two flows
/// (IngestInboundEmailCommand, RegisterTenantCommand) only learn which
/// tenant they're operating on partway through the request, after the
/// connection is already open. A per-connection cache skips the extra
/// round trip on every other request, where the tenant is already known
/// from the JWT before the first query ever runs.
///
/// EF Core's own global query filters (layer 1, see
/// ApplicationDbContext.OnModelCreating) already scope every query -
/// this is the belt-and-suspenders backstop that still holds even if a
/// query somewhere forgets to apply, or explicitly bypasses, the EF
/// filter.
/// </summary>
public class TenantSessionInterceptor : DbCommandInterceptor
{
    private readonly ICurrentTenantService _currentTenantService;
    private static readonly ConditionalWeakTable<DbConnection, StrongBox<Guid?>> LastAppliedTenant = new();

    public TenantSessionInterceptor(ICurrentTenantService currentTenantService)
    {
        _currentTenantService = currentTenantService;
    }

    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        await EnsureTenantSessionSetAsync(command, cancellationToken);
        return await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        await EnsureTenantSessionSetAsync(command, cancellationToken);
        return await base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override async ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        await EnsureTenantSessionSetAsync(command, cancellationToken);
        return await base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        EnsureTenantSessionSetAsync(command, CancellationToken.None).GetAwaiter().GetResult();
        return base.ReaderExecuting(command, eventData, result);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        EnsureTenantSessionSetAsync(command, CancellationToken.None).GetAwaiter().GetResult();
        return base.NonQueryExecuting(command, eventData, result);
    }

    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
    {
        EnsureTenantSessionSetAsync(command, CancellationToken.None).GetAwaiter().GetResult();
        return base.ScalarExecuting(command, eventData, result);
    }

    private async Task EnsureTenantSessionSetAsync(DbCommand command, CancellationToken cancellationToken)
    {
        var connection = command.Connection;
        if (connection is null)
        {
            return;
        }

        // Guid.Empty covers PlatformSuperAdmin / no-tenant contexts
        // (webhooks before they resolve a Mailbox, auth endpoints) -
        // policies never match it against a real tenant_id, so RLS
        // simply returns zero rows for those, same as the EF filter.
        var tenantId = _currentTenantService.TenantId ?? Guid.Empty;

        // box.Value starts as null (never explicitly set on this
        // connection yet) - deliberately distinct from Guid.Empty, so
        // the very first command on a fresh connection always issues
        // the SET even when the desired tenant happens to be Guid.Empty.
        // Skipping it in that case would leave Postgres's session
        // variable actually unset (NULL), and NULL fails every RLS
        // comparison - including the one a PlatformSuperAdmin's own
        // TeamMember row needs to match, which would break login itself.
        var box = LastAppliedTenant.GetOrCreateValue(connection);
        if (box.Value.HasValue && box.Value.Value == tenantId)
        {
            return;
        }

        await using var setCommand = connection.CreateCommand();
        if (command.Transaction is not null)
        {
            setCommand.Transaction = command.Transaction;
        }
        setCommand.CommandText = "SELECT set_config('app.current_tenant_id', @tenantId, false)";

        var parameter = setCommand.CreateParameter();
        parameter.ParameterName = "tenantId";
        parameter.Value = tenantId.ToString();
        setCommand.Parameters.Add(parameter);

        await setCommand.ExecuteNonQueryAsync(cancellationToken);

        box.Value = tenantId;
    }
}
