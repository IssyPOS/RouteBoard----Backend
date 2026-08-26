using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Common.Models;

namespace POSShopTicketing.Application.Platform.Queries.GetTenants;

/// <summary>PlatformSuperAdmin only - lists every Tenant on the
/// platform. Deliberately bypasses the tenant global query filter (the
/// Tenant entity itself has none) since this endpoint's whole purpose is
/// cross-tenant visibility.</summary>
public record GetTenantsQuery : IRequest<PaginatedList<TenantDto>>
{
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetTenantsQueryHandler : IRequestHandler<GetTenantsQuery, PaginatedList<TenantDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTenantsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<TenantDto>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Tenants
            .Include(t => t.TeamMembers)
            .AsNoTracking()
            // Guid.Empty is the reserved "Platform" row PlatformSuperAdmin
            // accounts are anchored to for referential integrity (see
            // ApplicationDbContextInitializer.EnsurePlatformTenantRowAsync)
            // - never a real customer, so never shown here.
            .Where(t => t.Id != Guid.Empty)
            .OrderBy(t => t.Name)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(t => EF.Functions.Like(t.Name, $"%{term}%") || EF.Functions.Like(t.Slug, $"%{term}%"));
        }

        var paged = await PaginatedList<Domain.Entities.Tenant>.CreateAsync(query, request.PageNumber, request.PageSize);
        var dtos = paged.Items.Select(TenantDto.FromEntity).ToList();

        return new PaginatedList<TenantDto>(dtos, paged.TotalCount, paged.PageNumber, request.PageSize);
    }
}
