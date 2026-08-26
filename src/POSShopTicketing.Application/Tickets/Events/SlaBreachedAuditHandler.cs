using MediatR;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Common.Models;

namespace POSShopTicketing.Application.Tickets.Events;

/// <summary>Always raised by the Hangfire SLA sweep, never by a request
/// - the actor is always "System".</summary>
public class SlaBreachedAuditHandler : INotificationHandler<SlaBreachedEvent>
{
    private readonly IAuditTrailService _auditTrailService;

    public SlaBreachedAuditHandler(IAuditTrailService auditTrailService)
    {
        _auditTrailService = auditTrailService;
    }

    public Task Handle(SlaBreachedEvent notification, CancellationToken cancellationToken) =>
        _auditTrailService.RecordAsync(new AuditTrailEntry
        {
            TenantId = notification.TenantId,
            ActorTeamMemberId = null,
            ActorDisplayName = "System",
            Action = "Ticket.SlaBreached",
            EntityType = nameof(Domain.Entities.Ticket),
            EntityId = notification.TicketId.ToString(),
            Summary = $"Ticket {notification.TicketNumber} missed its SLA due date."
        }, cancellationToken);
}
