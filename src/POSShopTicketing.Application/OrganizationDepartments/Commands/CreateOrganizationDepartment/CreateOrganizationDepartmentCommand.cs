using MediatR;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace POSShopTicketing.Application.OrganizationTeams.Commands.CreateOrganizationTeam;

public record CreateOrganizationDepartmentCommand : IRequest<Guid>
{
    public Guid OrganizationId { get; init; }
    public string Name { get; init; } = string.Empty;
}

public class CreateOrganizationDepartmentCommandHandler : IRequestHandler<CreateOrganizationDepartmentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _currentTenantService;

    public CreateOrganizationDepartmentCommandHandler(IApplicationDbContext context, ICurrentTenantService currentTenantService)
    {
        _context = context;
        _currentTenantService = currentTenantService;
    }

    public async Task<Guid> Handle(CreateOrganizationDepartmentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenantService.TenantId
            ?? throw new ForbiddenException("Organization teams must be created from within a tenant.");

        var organization = await _context.Organizations.FindAsync(new object[] { request.OrganizationId }, cancellationToken)
            ?? throw new NotFoundException(nameof(Organization), request.OrganizationId);

        var duplicateName = await _context.OrganizationDepartments
            .AsNoTracking()
            .AnyAsync(t => t.OrganizationId == request.OrganizationId && t.Name == request.Name.Trim(), cancellationToken);

        if (duplicateName)
        {
            throw new DomainException($"\"{request.Name}\" already exists as a team on this organization.");
        }

        var entity = new OrganizationDepartment
        {
            TenantId = tenantId,
            OrganizationId = organization.Id,
            Name = request.Name.Trim()
        };

        _context.OrganizationDepartments.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
