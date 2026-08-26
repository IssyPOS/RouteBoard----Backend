using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Common.Models;

namespace POSShopTicketing.Application.Tickets.Events;

public class TicketAssignedAuditHandler : INotificationHandler<TicketAssignedEvent>
{
    private readonly IAuditTrailService _auditTrailService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IApplicationDbContext _context;

    public TicketAssignedAuditHandler(
        IAuditTrailService auditTrailService, ICurrentUserService currentUserService, IApplicationDbContext context)
    {
        _auditTrailService = auditTrailService;
        _currentUserService = currentUserService;
        _context = context;
    }

    public async Task Handle(TicketAssignedEvent notification, CancellationToken cancellationToken)
    {
        // No Team Member context here means an Assignment Rule fired
        // automatically (new ticket via the inbound-email webhook) -
        // that's a system decision, not something to attribute to the
        // customer who happened to trigger it by emailing in.
        var (teamMemberId, actorDisplayName) = await TicketAuditActor.ResolveAsync(
            notification.TicketId, _currentUserService, _context,
            systemFallbackDisplayName: "Automated assignment rule", useCustomerEmailAsFallback: false, cancellationToken);

        var assigneeName = await _context.TeamMembers
            .AsNoTracking()
            .Where(u => u.Id == notification.AssignedToTeamMemberId)
            .Select(u => u.FirstName + " " + u.LastName)
            .FirstOrDefaultAsync(cancellationToken) ?? notification.AssignedToTeamMemberId.ToString();

        await _auditTrailService.RecordAsync(new AuditTrailEntry
        {
            TenantId = notification.TenantId,
            ActorTeamMemberId = teamMemberId,
            ActorDisplayName = actorDisplayName,
            Action = "Ticket.Assigned",
            EntityType = nameof(Domain.Entities.Ticket),
            EntityId = notification.TicketId.ToString(),
            Summary = $"{actorDisplayName} assigned ticket {notification.TicketNumber} to {assigneeName}."
        }, cancellationToken);
    }
}
