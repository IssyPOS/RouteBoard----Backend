using System.Data;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Infrastructure.Persistence;

namespace POSShopTicketing.Infrastructure.Services;

/// <summary>
/// Produces "{TenantTicketPrefix}-{sequence}" (e.g. "SP123-000456") from
/// a single shared Postgres sequence (TicketNumberSequence, defined in
/// ApplicationDbContext.OnModelCreating). nextval() is atomic at the
/// database level, so this is safe under concurrent ticket creation
/// across every tenant - unlike a "read the last number and add one"
/// query, which can race or tie-break incorrectly.
/// </summary>
public class TicketNumberGenerator : ITicketNumberGenerator
{
    private readonly ApplicationDbContext _context;

    public TicketNumberGenerator(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> NextAsync(string tenantTicketPrefix, CancellationToken cancellationToken)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT nextval('\"TicketNumberSequence\"')";
        var result = await command.ExecuteScalarAsync(cancellationToken);

        return $"{tenantTicketPrefix}-{Convert.ToInt64(result):D6}";
    }
}
