namespace POSShopTicketing.Application.Common.Interfaces;

/// <summary>Produces the human-friendly TicketNumber (e.g.
/// "SP123-000456") shown to agents and in email subject lines instead of
/// the internal UUID.</summary>
public interface ITicketNumberGenerator
{
    Task<string> NextAsync(string tenantTicketPrefix, CancellationToken cancellationToken);
}
