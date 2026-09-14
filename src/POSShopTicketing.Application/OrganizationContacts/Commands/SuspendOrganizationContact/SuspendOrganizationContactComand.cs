using MediatR;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;
using POSShopTicketing.Domain.Exceptions;

namespace POSShopTicketing.Application.Organizations.Commands.SuspendOrganizationContact;

public record SuspendOrganizationContactComand(Guid OrganizationContactId) : IRequest;

public class SuspendOrganizationDepartmentCommandHandler : IRequestHandler<SuspendOrganizationContactComand>
{
    private readonly IApplicationDbContext _context;

    public SuspendOrganizationDepartmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(SuspendOrganizationContactComand request, CancellationToken cancellationToken)
    {
        if (request.OrganizationContactId == Guid.Empty)
        {
            throw new DomainException("The reserved Organization Contact cannot be suspended.");
        }

        var organization = await _context.OrganizationContacts.FindAsync(new object[] { request.OrganizationContactId }, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationContact), request.OrganizationContactId);

        organization.Status = OrganizationContactStatus.Suspended;
        await _context.SaveChangesAsync(cancellationToken);
    }
}



