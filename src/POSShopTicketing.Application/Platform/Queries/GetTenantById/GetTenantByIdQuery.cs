using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Platform.Queries.GetTenants;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Application.Common.Interfaces;

namespace POSShopTicketing.Application.Platform.Queries.GetTenantById;

public record GetTenantByIdQuery(Guid Id) : IRequest<TenantDto>;

public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, TenantDto>
{
    private readonly IApplicationDbContext _context;

    public GetTenantByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TenantDto> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Tenants
            .Include(t => t.TeamMembers)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Tenant), request.Id);

        return TenantDto.FromEntity(entity);
    }
}
