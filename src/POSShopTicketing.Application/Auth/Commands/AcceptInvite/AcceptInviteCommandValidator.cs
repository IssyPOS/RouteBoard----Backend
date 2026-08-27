using FluentValidation;
using MediatR;

namespace POSShopTicketing.Application.Auth.Commands.AcceptInvite;

public class AcceptInviteCommandValidator : AbstractValidator<AcceptInviteCommand>
{
    public AcceptInviteCommandValidator()
    {
        RuleFor(v => v.InviteToken).NotEmpty();
        RuleFor(v => v.Password).NotEmpty().MinimumLength(8).MaximumLength(100);
        RuleFor(v => v.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(v => v.LastName).NotEmpty().MaximumLength(100);
    }
}
