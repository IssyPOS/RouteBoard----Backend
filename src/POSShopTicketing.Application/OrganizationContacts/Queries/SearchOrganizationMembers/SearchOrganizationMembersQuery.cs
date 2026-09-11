using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Interfaces;

namespace POSShopTicketing.Application.OrganizationMembers.Queries.SearchOrganizationMembers;

public record OrganizationMemberLookupDto(Guid Id, string FullName, string Email, Guid OrganizationId, string? OrganizationName);

/// <summary>Cheap typeahead for triage's "Link to existing member"
/// action and for manual ticket creation.</summary>
public record SearchOrganizationMembersQuery(string? Term, int MaxResults = 10) : IRequest<List<OrganizationMemberLookupDto>>;

public class SearchOrganizationMembersQueryHandler
    : IRequestHandler<SearchOrganizationMembersQuery, List<OrganizationMemberLookupDto>>
{
    private readonly IApplicationDbContext _context;

    public SearchOrganizationMembersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrganizationMemberLookupDto>> Handle(SearchOrganizationMembersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.OrganizationContacts
            .Include(m => m.Organization)
            .AsNoTracking()
            .OrderBy(m => m.LastName)
            .ThenBy(m => m.FirstName)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Term))
        {
            var term = request.Term.Trim();
            query = query.Where(m =>
    EF.Functions.Like(m.FirstName, $"%{term}%") ||
    EF.Functions.Like(m.LastName, $"%{term}%") ||
    EF.Functions.Like(m.FirstName + " " + m.LastName, $"%{term}%") ||
    EF.Functions.Like(m.Email, $"%{term}%"));
        }

        var results = await query.Take(request.MaxResults <= 0 ? 10 : request.MaxResults).ToListAsync(cancellationToken);

        return results
            .Select(m => new OrganizationMemberLookupDto(m.Id, m.LastName, m.Email, m.OrganizationId, m.Organization?.Name))
            .ToList();
    }
}
