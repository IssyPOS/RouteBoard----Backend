using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Interfaces;

namespace POSShopTicketing.Application.AssignmentRules.Queries.GetAssignmentRules;

public record GetAssignmentRulesQuery : IRequest<List<AssignmentRuleDto>>;

public class GetAssignmentRulesQueryHandler : IRequestHandler<GetAssignmentRulesQuery, List<AssignmentRuleDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAssignmentRulesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AssignmentRuleDto>> Handle(GetAssignmentRulesQuery request, CancellationToken cancellationToken)
    {
        var rules = await _context.AssignmentRules
            .Include(r => r.AssignedToTeamMember)
            .AsNoTracking()
            .OrderBy(r => r.ScopeType).ThenBy(r => r.PriorityOrder)
            .ToListAsync(cancellationToken);

        return rules.Select(AssignmentRuleDto.FromEntity).ToList();
    }
}
