using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Interfaces;

namespace POSShopTicketing.Application.OrganizationTeams.Queries.GetOrganizationTeams;

public record GetOrganizationTeamsQuery(Guid OrganizationId) : IRequest<List<OrganizationTeamDto>>;

public class GetOrganizationTeamsQueryHandler : IRequestHandler<GetOrganizationTeamsQuery, List<OrganizationTeamDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOrganizationTeamsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrganizationTeamDto>> Handle(GetOrganizationTeamsQuery request, CancellationToken cancellationToken)
    {
        var teams = await _context.OrganizationTeams
            .Include(t => t.Members)
            .AsNoTracking()
            .Where(t => t.OrganizationId == request.OrganizationId)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        return teams.Select(OrganizationTeamDto.FromEntity).ToList();
    }
}
