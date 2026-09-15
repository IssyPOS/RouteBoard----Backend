using MediatR;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;
using POSShopTicketing.Domain.Exceptions;

namespace POSShopTicketing.Application.Organizations.Commands.ReactivateOrganizationContact;

public record ReactivateOrganizationContactCommand(Guid OrganizationContactId) : IRequest;

public class ReactivateOrganizationContactCommandHandler : IRequestHandler<ReactivateOrganizationContactCommand>
{
    private readonly IApplicationDbContext _context;

    public ReactivateOrganizationContactCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ReactivateOrganizationContactCommand request, CancellationToken cancellationToken)
    {
        if (request.OrganizationContactId == Guid.Empty)
        {
            throw new DomainException("The reserved Organization Contact does not need reactivating.");
        }

        var organization = await _context.OrganizationContacts.FindAsync(new object[] { request.OrganizationContactId }, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationContact), request.OrganizationContactId);

        organization.Status = OrganizationContactStatus.Active;
        await _context.SaveChangesAsync(cancellationToken);
    }
}



