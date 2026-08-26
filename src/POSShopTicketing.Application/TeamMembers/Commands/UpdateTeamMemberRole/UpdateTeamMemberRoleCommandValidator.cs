using FluentValidation;

namespace POSShopTicketing.Application.TeamMembers.Commands.UpdateTeamMemberRole;

public class UpdateTeamMemberRoleCommandValidator : AbstractValidator<UpdateTeamMemberRoleCommand>
{
    public UpdateTeamMemberRoleCommandValidator()
    {
        RuleFor(v => v.TeamMemberId).NotEmpty();
        RuleFor(v => v.NewRole).IsInEnum();
    }
}
