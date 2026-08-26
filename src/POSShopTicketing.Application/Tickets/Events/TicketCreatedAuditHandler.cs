using MediatR;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Common.Models;

namespace POSShopTicketing.Application.Tickets.Events;

/// <summary>Separate from TicketCreatedEventHandler (which drives the
/// in-app Notification/alert) - this one only feeds the audit
/// trail.</summary>
public class TicketCreatedAuditHandler : INotificationHandler<TicketCreatedEvent>
{
    private readonly IAuditTrailService _auditTrailService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IApplicationDbContext _context;

    public TicketCreatedAuditHandler(
        IAuditTrailService auditTrailService, ICurrentUserService currentUserService, IApplicationDbContext context)
    {
        _auditTrailService = auditTrailService;
        _currentUserService = currentUserService;
        _context = context;
    }

    public async Task Handle(TicketCreatedEvent notification, CancellationToken cancellationToken)
    {
        var (teamMemberId, actorDisplayName) = await TicketAuditActor.ResolveAsync(
            notification.TicketId, _currentUserService, _context,
            systemFallbackDisplayName: "System", useCustomerEmailAsFallback: true, cancellationToken);

        var statusNote = notification.IsUnverified ? " (Unverified - awaiting triage)" : string.Empty;

        await _auditTrailService.RecordAsync(new AuditTrailEntry
        {
            TenantId = notification.TenantId,
            ActorTeamMemberId = teamMemberId,
            ActorDisplayName = actorDisplayName,
            Action = "Ticket.Created",
            EntityType = nameof(Domain.Entities.Ticket),
            EntityId = notification.TicketId.ToString(),
            Summary = $"{actorDisplayName} created ticket {notification.TicketNumber}: \"{notification.Subject}\"{statusNote}."
        }, cancellationToken);
    }
}
