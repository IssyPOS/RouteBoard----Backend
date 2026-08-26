using MediatR;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Common.Models;

namespace POSShopTicketing.Application.Tickets.Events;

public class TicketStatusChangedAuditHandler : INotificationHandler<TicketStatusChangedEvent>
{
    private readonly IAuditTrailService _auditTrailService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IApplicationDbContext _context;

    public TicketStatusChangedAuditHandler(
        IAuditTrailService auditTrailService, ICurrentUserService currentUserService, IApplicationDbContext context)
    {
        _auditTrailService = auditTrailService;
        _currentUserService = currentUserService;
        _context = context;
    }

    public async Task Handle(TicketStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        // A status change with no Team Member context is the one
        // automated case: a customer reply reopening a Resolved/Closed
        // ticket via the inbound-email webhook.
        var (teamMemberId, actorDisplayName) = await TicketAuditActor.ResolveAsync(
            notification.TicketId, _currentUserService, _context,
            systemFallbackDisplayName: "System", useCustomerEmailAsFallback: true, cancellationToken);

        await _auditTrailService.RecordAsync(new AuditTrailEntry
        {
            TenantId = notification.TenantId,
            ActorTeamMemberId = teamMemberId,
            ActorDisplayName = actorDisplayName,
            Action = "Ticket.StatusChanged",
            EntityType = nameof(Domain.Entities.Ticket),
            EntityId = notification.TicketId.ToString(),
            Summary = $"{actorDisplayName} moved ticket {notification.TicketNumber} from " +
                      $"{notification.FromStatus} to {notification.ToStatus}."
        }, cancellationToken);
    }
}
