using MediatR;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.TeamMembers.Commands.UpdateTeamMemberRole;

/// <summary>Owner/Admin only. An Admin can't grant Owner or another
/// Admin - only an existing Owner can (enforced here rather than only at
/// the controller, since "who can promote to what" is a business rule,
/// not just a route guard).</summary>
public record UpdateTeamMemberRoleCommand(Guid TeamMemberId, TeamMemberRole NewRole) : IRequest;

public class UpdateTeamMemberRoleCommandHandler : IRequestHandler<UpdateTeamMemberRoleCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateTeamMemberRoleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateTeamMemberRoleCommand request, CancellationToken cancellationToken)
    {
        if (request.NewRole == TeamMemberRole.PlatformSuperAdmin)
        {
            throw new ForbiddenException("PlatformSuperAdmin cannot be granted this way.");
        }

        var isPromotingToLeadership = request.NewRole is TeamMemberRole.Owner or TeamMemberRole.Admin;
        if (isPromotingToLeadership && _currentUserService.Role != TeamMemberRole.Owner)
        {
            throw new ForbiddenException("Only an Owner can grant Owner or Admin.");
        }

        var teamMember = await _context.TeamMembers.FindAsync(new object[] { request.TeamMemberId }, cancellationToken)
            ?? throw new NotFoundException(nameof(TeamMember), request.TeamMemberId);

        teamMember.Role = request.NewRole;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
