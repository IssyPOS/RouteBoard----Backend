using FluentValidation;

namespace POSShopTicketing.Application.AssignmentRules.Commands.CreateAssignmentRule;

public class CreateAssignmentRuleCommandValidator : AbstractValidator<CreateAssignmentRuleCommand>
{
    public CreateAssignmentRuleCommandValidator()
    {
        RuleFor(v => v.ScopeType).IsInEnum();
        RuleFor(v => v.ScopeId).NotEmpty();
        RuleFor(v => v.AssignedToTeamMemberId).NotEmpty();
        RuleFor(v => v.PriorityOrder).GreaterThanOrEqualTo(0);
    }
}
