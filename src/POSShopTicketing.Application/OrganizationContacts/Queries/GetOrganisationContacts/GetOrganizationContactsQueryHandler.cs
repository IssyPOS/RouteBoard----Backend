using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Common.Models;
using POSShopTicketing.Application.OrganizationMembers.Queries.GetOrganizationMembers;

namespace POSShopTicketing.Application.OrganizationContacts.Queries.GetOrganizationContacts;

public class GetOrganizationContactsQueryHandler
    : IRequestHandler<GetOrganizationContactsQuery, PaginatedList<OrganizationContactDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOrganizationContactsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<OrganizationContactDto>> Handle(
        GetOrganizationContactsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.OrganizationContacts
            .Include(c => c.Organization)
            .Include(c => c.OrganizationDepartment)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();

            query = query.Where(c =>
                EF.Functions.Like(c.FullName, $"%{term}%") ||
                EF.Functions.Like(c.Email, $"%{term}%") ||
                EF.Functions.Like(c.Organization!.Name, $"%{term}%"));
        }

        query = query.OrderBy(c => c.FullName);

        var paged = await PaginatedList<Domain.Entities.OrganizationContact>
            .CreateAsync(
                query,
                request.PageNumber,
                request.PageSize);

        var dtos = paged.Items
            .Select(OrganizationContactDto.FromEntity)
            .ToList();

        return new PaginatedList<OrganizationContactDto>(
            dtos,
            paged.TotalCount,
            paged.PageNumber,
            request.PageSize);
    }
}