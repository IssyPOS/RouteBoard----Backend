using FluentValidation;

namespace POSShopTicketing.Application.Auth.Commands.Logout;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(v => v.RefreshToken).NotEmpty();
    }
}
