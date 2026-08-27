using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Common.Models;

namespace POSShopTicketing.Application.OrganizationMembers.Queries.GetOrganizationMembers;

public record GetOrganizationContactsQuery : IRequest<PaginatedList<OrganizationContactDto>>
{
    public Guid? OrganizationId { get; init; }
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetOrganizationContactsQueryHandler : IRequestHandler<GetOrganizationContactsQuery, PaginatedList<OrganizationContactDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOrganizationContactsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<OrganizationContactDto>> Handle(GetOrganizationContactsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.OrganizationContacts
            .Include(m => m.Organization)
            .Include(m => m.OrganizationDepartment)
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

        var paged = await PaginatedList<Domain.Entities.OrganizationContact>.CreateAsync(query, request.PageNumber, request.PageSize);
        var dtos = paged.Items.Select(OrganizationContactDto.FromEntity).ToList();

        return new PaginatedList<OrganizationContactDto>(dtos, paged.TotalCount, paged.PageNumber, request.PageSize);
    }
}
