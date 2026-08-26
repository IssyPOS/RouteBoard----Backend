using MediatR;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Common.Models;

namespace POSShopTicketing.Application.Auth.Events;

public class TeamMemberLoggedInEventHandler : INotificationHandler<TeamMemberLoggedInEvent>
{
    private readonly IAuditTrailService _auditTrailService;

    public TeamMemberLoggedInEventHandler(IAuditTrailService auditTrailService)
    {
        _auditTrailService = auditTrailService;
    }

    public Task Handle(TeamMemberLoggedInEvent notification, CancellationToken cancellationToken) =>
        _auditTrailService.RecordAsync(new AuditTrailEntry
        {
            TenantId = notification.TenantId,
            ActorTeamMemberId = notification.TeamMemberId,
            ActorDisplayName = notification.FullName,
            Action = "TeamMember.LoggedIn",
            EntityType = nameof(Domain.Entities.TeamMember),
            EntityId = notification.TeamMemberId.ToString(),
            Summary = $"{notification.FullName} ({notification.Email}, {notification.Role}) logged in."
        }, cancellationToken);
}
