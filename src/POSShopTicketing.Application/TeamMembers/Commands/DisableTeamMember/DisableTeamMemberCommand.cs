using MediatR;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;
using POSShopTicketing.Domain.Exceptions;

namespace POSShopTicketing.Application.TeamMembers.Commands.DisableTeamMember;

/// <summary>Owner/Admin only. Per the spec's roles table, an Owner can
/// never be removed from the tenant - so disabling one is rejected
/// outright rather than silently locking a tenant out of its own
/// account.</summary>
public record DisableTeamMemberCommand(Guid TeamMemberId) : IRequest;

public class DisableTeamMemberCommandHandler : IRequestHandler<DisableTeamMemberCommand>
{
    private readonly IApplicationDbContext _context;

    public DisableTeamMemberCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DisableTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var teamMember = await _context.TeamMembers.FindAsync(new object[] { request.TeamMemberId }, cancellationToken)
            ?? throw new NotFoundException(nameof(TeamMember), request.TeamMemberId);

        if (teamMember.Role == TeamMemberRole.Owner)
        {
            throw new DomainException("The Owner cannot be removed from the tenant.");
        }

        teamMember.Status = TeamMemberStatus.Disabled;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
