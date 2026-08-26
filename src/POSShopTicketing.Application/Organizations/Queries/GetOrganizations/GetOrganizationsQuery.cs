using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Common.Models;

namespace POSShopTicketing.Application.Organizations.Queries.GetOrganizations;

public record GetOrganizationsQuery : IRequest<PaginatedList<OrganizationDto>>
{
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetOrganizationsQueryHandler : IRequestHandler<GetOrganizationsQuery, PaginatedList<OrganizationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOrganizationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<OrganizationDto>> Handle(GetOrganizationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Organizations
            .Include(o => o.Teams)
            .Include(o => o.Members)
            .AsNoTracking()
            .OrderBy(o => o.Name)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(o => EF.Functions.Like(o.Name, $"%{term}%"));
        }

        var paged = await PaginatedList<Domain.Entities.Organization>.CreateAsync(query, request.PageNumber, request.PageSize);
        var dtos = paged.Items.Select(OrganizationDto.FromEntity).ToList();

        return new PaginatedList<OrganizationDto>(dtos, paged.TotalCount, paged.PageNumber, request.PageSize);
    }
}
