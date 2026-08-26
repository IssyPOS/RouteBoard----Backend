using MediatR;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Entities;

namespace POSShopTicketing.Application.Organizations.Commands.CreateOrganization;

public record CreateOrganizationCommand : IRequest<Guid>
{
    public string Name { get; init; } = string.Empty;
}

public class CreateOrganizationCommandHandler : IRequestHandler<CreateOrganizationCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _currentTenantService;

    public CreateOrganizationCommandHandler(IApplicationDbContext context, ICurrentTenantService currentTenantService)
    {
        _context = context;
        _currentTenantService = currentTenantService;
    }

    public async Task<Guid> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenantService.TenantId
            ?? throw new ForbiddenException("Organizations must be created from within a tenant.");

        var entity = new Organization
        {
            TenantId = tenantId,
            Name = request.Name.Trim()
        };

        _context.Organizations.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
