using MediatR;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Common.Models;

namespace POSShopTicketing.Application.Tickets.Events;

/// <summary>Always fires from an authenticated Manager+ request (PATCH
/// /tickets/{id}/escalate) - no automated/webhook path for this one, so
/// no fallback needed.</summary>
public class TicketEscalatedAuditHandler : INotificationHandler<TicketEscalatedEvent>
{
    private readonly IAuditTrailService _auditTrailService;
    private readonly ICurrentUserService _currentUserService;

    public TicketEscalatedAuditHandler(IAuditTrailService auditTrailService, ICurrentUserService currentUserService)
    {
        _auditTrailService = auditTrailService;
        _currentUserService = currentUserService;
    }

    public Task Handle(TicketEscalatedEvent notification, CancellationToken cancellationToken)
    {
        var actorDisplayName = _currentUserService.Email ?? "Unknown";

        return _auditTrailService.RecordAsync(new AuditTrailEntry
        {
            TenantId = notification.TenantId,
            ActorTeamMemberId = _currentUserService.TeamMemberId,
            ActorDisplayName = actorDisplayName,
            Action = "Ticket.Escalated",
            EntityType = nameof(Domain.Entities.Ticket),
            EntityId = notification.TicketId.ToString(),
            Summary = $"{actorDisplayName} escalated ticket {notification.TicketNumber}."
        }, cancellationToken);
    }
}
