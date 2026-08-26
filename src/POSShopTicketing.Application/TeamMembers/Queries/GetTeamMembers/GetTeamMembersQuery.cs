using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Common.Models;

namespace POSShopTicketing.Application.TeamMembers.Queries.GetTeamMembers;

public record GetTeamMembersQuery : IRequest<PaginatedList<TeamMemberDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetTeamMembersQueryHandler : IRequestHandler<GetTeamMembersQuery, PaginatedList<TeamMemberDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTeamMembersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<TeamMemberDto>> Handle(GetTeamMembersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TeamMembers
            .Include(u => u.AssignedTickets)
            .AsNoTracking()
            .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
            .AsQueryable();

        var paged = await PaginatedList<Domain.Entities.TeamMember>.CreateAsync(query, request.PageNumber, request.PageSize);
        var dtos = paged.Items.Select(TeamMemberDto.FromEntity).ToList();

        return new PaginatedList<TeamMemberDto>(dtos, paged.TotalCount, paged.PageNumber, request.PageSize);
    }
}
