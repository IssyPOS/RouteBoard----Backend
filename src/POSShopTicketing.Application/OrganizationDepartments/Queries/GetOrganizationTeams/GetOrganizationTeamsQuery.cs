using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Interfaces;

namespace POSShopTicketing.Application.OrganizationTeams.Queries.GetOrganizationTeams;

public record GetOrganizationDepartmentsQuery(Guid OrganizationId) : IRequest<List<OrganizationDepartmentDto>>;

public class GetOrganizationTeamsQueryHandler : IRequestHandler<GetOrganizationDepartmentsQuery, List<OrganizationDepartmentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOrganizationTeamsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrganizationDepartmentDto>> Handle(GetOrganizationDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var teams = await _context.OrganizationDepartments
            .Include(t => t.Members)
            .AsNoTracking()
            .Where(t => t.OrganizationId == request.OrganizationId)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        return teams.Select(OrganizationDepartmentDto.FromEntity).ToList();
    }
}
