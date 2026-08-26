using MediatR;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.AssignmentRules.Commands.CreateAssignmentRule;

public record CreateAssignmentRuleCommand : IRequest<Guid>
{
    public AssignmentScopeType ScopeType { get; init; }
    public Guid ScopeId { get; init; }
    public Guid AssignedToTeamMemberId { get; init; }
    public int PriorityOrder { get; init; }
}

public class CreateAssignmentRuleCommandHandler : IRequestHandler<CreateAssignmentRuleCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _currentTenantService;

    public CreateAssignmentRuleCommandHandler(IApplicationDbContext context, ICurrentTenantService currentTenantService)
    {
        _context = context;
        _currentTenantService = currentTenantService;
    }

    public async Task<Guid> Handle(CreateAssignmentRuleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenantService.TenantId
            ?? throw new ForbiddenException("Assignment rules must be created from within a tenant.");

        var assignee = await _context.TeamMembers.FindAsync(new object[] { request.AssignedToTeamMemberId }, cancellationToken)
            ?? throw new NotFoundException(nameof(TeamMember), request.AssignedToTeamMemberId);

        var entity = new AssignmentRule
        {
            TenantId = tenantId,
            ScopeType = request.ScopeType,
            ScopeId = request.ScopeId,
            AssignedToTeamMemberId = assignee.Id,
            PriorityOrder = request.PriorityOrder,
            IsActive = true
        };

        _context.AssignmentRules.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
