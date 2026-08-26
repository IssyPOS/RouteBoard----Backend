using MediatR;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Common.Models;

namespace POSShopTicketing.Application.Auth.Events;

public class TeamMemberLoggedOutEventHandler : INotificationHandler<TeamMemberLoggedOutEvent>
{
    private readonly IAuditTrailService _auditTrailService;

    public TeamMemberLoggedOutEventHandler(IAuditTrailService auditTrailService)
    {
        _auditTrailService = auditTrailService;
    }

    public Task Handle(TeamMemberLoggedOutEvent notification, CancellationToken cancellationToken) =>
        _auditTrailService.RecordAsync(new AuditTrailEntry
        {
            TenantId = notification.TenantId,
            ActorTeamMemberId = notification.TeamMemberId,
            ActorDisplayName = notification.FullName,
            Action = "TeamMember.LoggedOut",
            EntityType = nameof(Domain.Entities.TeamMember),
            EntityId = notification.TeamMemberId.ToString(),
            Summary = $"{notification.FullName} ({notification.Email}, {notification.Role}) logged out."
        }, cancellationToken);
}
