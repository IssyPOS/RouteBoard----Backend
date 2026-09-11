using MediatR;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;
using POSShopTicketing.Domain.Exceptions;

namespace POSShopTicketing.Application.Organizations.Commands.ReactivateOrganization;

public record ReactivateOrganizationCommand(Guid OrganizationId) : IRequest;

public class ReactivateTenantCommandHandler : IRequestHandler<ReactivateOrganizationCommand>
{
    private readonly IApplicationDbContext _context;

    public ReactivateTenantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ReactivateOrganizationCommand request, CancellationToken cancellationToken)
    {
        if (request.OrganizationId == Guid.Empty)
        {
            throw new DomainException("The reserved Organization tenant does not need reactivating.");
        }

        var organization = await _context.Organizations.FindAsync(new object[] { request.OrganizationId }, cancellationToken)
            ?? throw new NotFoundException(nameof(Organization), request.OrganizationId);

        organization.Status = OrganizationStatus.Active;
        await _context.SaveChangesAsync(cancellationToken);
    }
}

