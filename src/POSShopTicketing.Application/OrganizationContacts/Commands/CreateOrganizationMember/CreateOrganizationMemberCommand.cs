using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Exceptions;

namespace POSShopTicketing.Application.OrganizationMembers.Commands.CreateOrganizationMember;

public record CreateOrganizationContactCommand : IRequest<Guid>
{
    public Guid OrganizationId { get; init; }
    public Guid? OrganizationTeamId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
}

public class CreateOrganizationContactCommandHandler : IRequestHandler<CreateOrganizationContactCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _currentTenantService;

    public CreateOrganizationContactCommandHandler(IApplicationDbContext context, ICurrentTenantService currentTenantService)
    {
        _context = context;
        _currentTenantService = currentTenantService;
    }

    public async Task<Guid> Handle(CreateOrganizationContactCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenantService.TenantId
            ?? throw new ForbiddenException("Organization members must be created from within a tenant.");

        var organization = await _context.Organizations.FindAsync(new object[] { request.OrganizationId }, cancellationToken)
            ?? throw new NotFoundException(nameof(Organization), request.OrganizationId);

        if (request.OrganizationTeamId.HasValue)
        {
            var team = await _context.OrganizationDepartments.FindAsync(new object[] { request.OrganizationTeamId.Value }, cancellationToken);
            if (team is null || team.OrganizationId != organization.Id)
            {
                throw new NotFoundException(nameof(OrganizationDepartment), request.OrganizationTeamId.Value);
            }
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var duplicate = await _context.OrganizationContacts
            .AsNoTracking()
            .AnyAsync(m => m.OrganizationId == request.OrganizationId && m.Email == normalizedEmail, cancellationToken);

        if (duplicate)
        {
            throw new DomainException($"\"{normalizedEmail}\" is already a member of this organization.");
        }

        var entity = new OrganizationContact
        {
            TenantId = tenantId,
            OrganizationId = organization.Id,
            OrganizationDepartmentId = request.OrganizationTeamId,
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            Phone = request.Phone
        };

        _context.OrganizationContacts.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
