using FluentValidation;

namespace POSShopTicketing.Application.Auth.Commands.RegisterTenant;

public class RegisterTenantCommandValidator : AbstractValidator<RegisterTenantCommand>
{
    public RegisterTenantCommandValidator()
    {
        RuleFor(v => v.TenantName).NotEmpty().MaximumLength(200);

        RuleFor(v => v.OwnerEmail).NotEmpty().EmailAddress().MaximumLength(200);

        RuleFor(v => v.OwnerPassword)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(100);

        RuleFor(v => v.OwnerFirstName).NotEmpty().MaximumLength(100);
        RuleFor(v => v.OwnerLastName).NotEmpty().MaximumLength(100);
    }
}
