using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;
using POSShopTicketing.Domain.Exceptions;

namespace POSShopTicketing.Application.OrganizationMembers.Commands.MergeOrganizationMember;

/// <summary>
/// Dedupes an alternate/typo'd address for someone already known:
/// POST /organizations/{id}/members/{memberId}/merge. Every Ticket
/// currently attributed to MemberId is re-pointed to MergeIntoMemberId,
/// then MemberId is deactivated (not hard-deleted, so ticket history
/// referencing it - if anything was missed - still resolves).
/// </summary>
public record MergeOrganizationMemberCommand(Guid OrganizationId, Guid MemberId, Guid MergeIntoMemberId) : IRequest;

public class MergeOrganizationMemberCommandHandler : IRequestHandler<MergeOrganizationMemberCommand>
{
    private readonly IApplicationDbContext _context;

    public MergeOrganizationMemberCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(MergeOrganizationMemberCommand request, CancellationToken cancellationToken)
    {
        if (request.MemberId == request.MergeIntoMemberId)
        {
            throw new DomainException("Cannot merge a member into itself.");
        }

        var duplicate = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.Id == request.MemberId && m.OrganizationId == request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationMember), request.MemberId);

        var primary = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.Id == request.MergeIntoMemberId && m.OrganizationId == request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationMember), request.MergeIntoMemberId);

        var ticketsToReassign = await _context.Tickets
            .Where(t => t.OrganizationMemberId == duplicate.Id)
            .ToListAsync(cancellationToken);

        foreach (var ticket in ticketsToReassign)
        {
            ticket.OrganizationMemberId = primary.Id;
            ticket.OrganizationTeamId ??= primary.OrganizationTeamId;
        }

        duplicate.Status = OrganizationMemberStatus.Inactive;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
