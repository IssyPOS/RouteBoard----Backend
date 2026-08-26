using FluentValidation;

namespace POSShopTicketing.Application.Auth.Commands.AcceptInvite;

public class AcceptInviteCommandValidator : AbstractValidator<AcceptInviteCommand>
{
    public AcceptInviteCommandValidator()
    {
        RuleFor(v => v.InviteToken).NotEmpty();
        RuleFor(v => v.Password).NotEmpty().MinimumLength(8).MaximumLength(100);
    }
}
