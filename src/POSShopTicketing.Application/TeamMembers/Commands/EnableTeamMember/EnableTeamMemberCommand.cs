using MediatR;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;
using POSShopTicketing.Domain.Exceptions;

namespace POSShopTicketing.Application.TeamMembers.Commands.EnableTeamMember;

/// <summary>Owner/Admin only. Per the spec's roles table, an Owner can
/// never be removed from the tenant - so disabling one is rejected
/// outright rather than silently locking a tenant out of its own
/// account.</summary>
public record EnableTeamMemberCommand(Guid TeamMemberId) : IRequest;

public class DisableTeamMemberCommandHandler : IRequestHandler<EnableTeamMemberCommand>
{
    private readonly IApplicationDbContext _context;

    public DisableTeamMemberCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(EnableTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var teamMember = await _context.TeamMembers.FindAsync(new object[] { request.TeamMemberId }, cancellationToken)
            ?? throw new NotFoundException(nameof(TeamMember), request.TeamMemberId);
               
        teamMember.Status = TeamMemberStatus.Active;
        await _context.SaveChangesAsync(cancellationToken);
    }
}

