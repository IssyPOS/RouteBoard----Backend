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
public record MergeOrganizationContactCommand(Guid OrganizationId, Guid ContactId, Guid MergeIntoContactId) : IRequest;

public class MergeOrganizationContactCommandHandler : IRequestHandler<MergeOrganizationContactCommand>
{
    private readonly IApplicationDbContext _context;

    public MergeOrganizationContactCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(MergeOrganizationContactCommand request, CancellationToken cancellationToken)
    {
        if (request.ContactId == request.MergeIntoContactId)
        {
            throw new DomainException("Cannot merge a member into itself.");
        }

        var duplicate = await _context.OrganizationContacts
            .FirstOrDefaultAsync(m => m.Id == request.ContactId && m.OrganizationId == request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationContact), request.ContactId);

        var primary = await _context.OrganizationContacts
            .FirstOrDefaultAsync(m => m.Id == request.MergeIntoContactId && m.OrganizationId == request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationContact), request.MergeIntoContactId);

        var ticketsToReassign = await _context.Tickets
            .Where(t => t.OrganizationContactId == duplicate.Id)
            .ToListAsync(cancellationToken);

        foreach (var ticket in ticketsToReassign)
        {
            ticket.OrganizationContactId = primary.Id;
            ticket.OrganizationDepartmentId ??= primary.OrganizationDepartmentId;
        }

        duplicate.Status = OrganizationContactStatus.Inactive;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
