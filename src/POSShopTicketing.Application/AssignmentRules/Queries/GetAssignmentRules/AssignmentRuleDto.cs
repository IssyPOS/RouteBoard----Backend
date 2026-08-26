using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.AssignmentRules.Queries.GetAssignmentRules;

public record AssignmentRuleDto
{
    public Guid Id { get; init; }
    public AssignmentScopeType ScopeType { get; init; }
    public Guid ScopeId { get; init; }
    public Guid AssignedToTeamMemberId { get; init; }
    public string? AssignedToTeamMemberName { get; init; }
    public int PriorityOrder { get; init; }
    public bool IsActive { get; init; }

    public static AssignmentRuleDto FromEntity(AssignmentRule entity) => new()
    {
        Id = entity.Id,
        ScopeType = entity.ScopeType,
        ScopeId = entity.ScopeId,
        AssignedToTeamMemberId = entity.AssignedToTeamMemberId,
        AssignedToTeamMemberName = entity.AssignedToTeamMember?.FullName,
        PriorityOrder = entity.PriorityOrder,
        IsActive = entity.IsActive
    };
}
