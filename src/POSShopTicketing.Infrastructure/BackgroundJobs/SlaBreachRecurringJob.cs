using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Tickets.Events;
using POSShopTicketing.Domain.Enums;
using POSShopTicketing.Infrastructure.Persistence;

namespace POSShopTicketing.Infrastructure.BackgroundJobs;

/// <summary>
/// SLA management: registered as a Hangfire recurring job (see
/// DependencyInjection.AddInfrastructure) rather than a custom polling
/// BackgroundService - "Background jobs ... for ... later SLA timers"
/// per the spec's architecture section. Runs across every tenant (a
/// system job, not scoped to any one request), and resolves ticket ->
/// tenant per row via SetTenant so RLS (if a hardened deployment
/// enforces it for the runtime role) stays correctly scoped for each
/// write.
///
/// Fires SlaBreachedEvent for tickets whose DueAt falls inside a rolling
/// lookback window rather than tracking a persisted "already notified"
/// flag (the spec's ticket schema doesn't have one) - generous enough to
/// tolerate a missed run, narrow enough not to re-alert indefinitely on
/// every subsequent sweep of a still-open, still-breached ticket.
/// </summary>
public class SlaBreachRecurringJob
{
    private static readonly TimeSpan LookbackWindow = TimeSpan.FromMinutes(10);

    private readonly ApplicationDbContext _context;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IPublisher _publisher;
    private readonly ILogger<SlaBreachRecurringJob> _logger;

    public SlaBreachRecurringJob(
        ApplicationDbContext context,
        ICurrentTenantService currentTenantService,
        IPublisher publisher,
        ILogger<SlaBreachRecurringJob> logger)
    {
        _context = context;
        _currentTenantService = currentTenantService;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var windowStart = now - LookbackWindow;

        var candidates = await _context.Tickets
            .IgnoreQueryFilters()
            .Where(t => t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed
                        && t.DueAt != null && t.DueAt >= windowStart && t.DueAt < now)
            .ToListAsync(cancellationToken);

        foreach (var ticket in candidates)
        {
            _currentTenantService.SetTenant(ticket.TenantId);
            await _publisher.Publish(new SlaBreachedEvent(ticket.TenantId, ticket.Id, ticket.TicketNumber), cancellationToken);
        }

        _logger.LogInformation("SLA sweep: {Count} ticket(s) newly past their due date", candidates.Count);
    }
}
