using MediatR;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;
using POSShopTicketing.Domain.Exceptions;

namespace POSShopTicketing.Application.Organizations.Commands.SuspendOrganizationDepartment;

public record SuspendOrganizationDepartmentCommand(Guid OrganizationId) : IRequest;

public class SuspendOrganizationDepartmentCommandHandler : IRequestHandler<SuspendOrganizationDepartmentCommand>
{
    private readonly IApplicationDbContext _context;

    public SuspendOrganizationDepartmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(SuspendOrganizationDepartmentCommand request, CancellationToken cancellationToken)
    {
        if (request.OrganizationId == Guid.Empty)
        {
            throw new DomainException("The reserved Organization Department cannot be suspended.");
        }

        var organization = await _context.OrganizationDepartments.FindAsync(new object[] { request.OrganizationId }, cancellationToken)
            ?? throw new NotFoundException(nameof(Organization), request.OrganizationId);

        organization.Status = OrganizationDepartmentStatus.Suspended;
        await _context.SaveChangesAsync(cancellationToken);
    }
}


