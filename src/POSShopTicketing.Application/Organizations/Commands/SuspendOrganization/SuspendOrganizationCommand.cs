using MediatR;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;
using POSShopTicketing.Domain.Exceptions;

namespace POSShopTicketing.Application.Organizations.Commands.SuspendOrganization;

public record SuspendOrganizationCommand(Guid OrganizationId) : IRequest;

public class SuspendTenantCommandHandler : IRequestHandler<SuspendOrganizationCommand>
{
    private readonly IApplicationDbContext _context;

    public SuspendTenantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(SuspendOrganizationCommand request, CancellationToken cancellationToken)
    {
        if (request.OrganizationId == Guid.Empty)
        {
            throw new DomainException("The reserved Organization cannot be suspended.");
        }

        var organization = await _context.Organizations.FindAsync(new object[] { request.OrganizationId }, cancellationToken)
            ?? throw new NotFoundException(nameof(Organization), request.OrganizationId);

        organization.Status = OrganizationStatus.Suspended;
        await _context.SaveChangesAsync(cancellationToken);
    }
}

