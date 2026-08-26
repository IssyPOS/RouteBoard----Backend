using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Common.Models;

namespace POSShopTicketing.Application.OrganizationMembers.Queries.GetOrganizationMembers;

public record GetOrganizationMembersQuery : IRequest<PaginatedList<OrganizationMemberDto>>
{
    public Guid? OrganizationId { get; init; }
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetOrganizationMembersQueryHandler : IRequestHandler<GetOrganizationMembersQuery, PaginatedList<OrganizationMemberDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOrganizationMembersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<OrganizationMemberDto>> Handle(GetOrganizationMembersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.OrganizationMembers
            .Include(m => m.Organization)
            .Include(m => m.OrganizationTeam)
            .AsNoTracking()
            .OrderBy(m => m.FullName)
            .AsQueryable();

        if (request.OrganizationId.HasValue)
        {
            query = query.Where(m => m.OrganizationId == request.OrganizationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(m => EF.Functions.Like(m.FullName, $"%{term}%") || EF.Functions.Like(m.Email, $"%{term}%"));
        }

        var paged = await PaginatedList<Domain.Entities.OrganizationMember>.CreateAsync(query, request.PageNumber, request.PageSize);
        var dtos = paged.Items.Select(OrganizationMemberDto.FromEntity).ToList();

        return new PaginatedList<OrganizationMemberDto>(dtos, paged.TotalCount, paged.PageNumber, request.PageSize);
    }
}
