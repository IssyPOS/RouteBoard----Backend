using FluentValidation;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Auth.Commands.InviteTeamMember;

public class InviteTeamMemberCommandValidator : AbstractValidator<InviteTeamMemberCommand>
{
    public InviteTeamMemberCommandValidator()
    {
        RuleFor(v => v.Email).NotEmpty().EmailAddress().MaximumLength(200);
        //RuleFor(v => v.FirstName).NotEmpty().MaximumLength(100);
        //RuleFor(v => v.LastName).NotEmpty().MaximumLength(100);
        RuleFor(v => v.Role).IsInEnum().NotEqual(TeamMemberRole.PlatformSuperAdmin);
    }
}
