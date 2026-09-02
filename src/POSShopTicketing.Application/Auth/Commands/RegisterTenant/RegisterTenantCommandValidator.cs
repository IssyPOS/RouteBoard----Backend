using FluentValidation;
using System.Net.Mail;

namespace POSShopTicketing.Application.Auth.Commands.RegisterTenant;

public class RegisterTenantCommandValidator : AbstractValidator<RegisterTenantCommand>
{
    public RegisterTenantCommandValidator()
    {
        RuleFor(v => v.TenantName).NotEmpty().MaximumLength(200);

        RuleFor(v => v.OwnerEmail).NotEmpty().EmailAddress().MaximumLength(200).Must(BeValidEmail).WithMessage("Please provide a valid email address.");

        RuleFor(v => v.OwnerPassword)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(100);

        RuleFor(v => v.OwnerFirstName).NotEmpty().MaximumLength(100);
        RuleFor(v => v.OwnerLastName).NotEmpty().MaximumLength(100);

    }

    private static bool BeValidEmail(string email)

    {

        try

        {

            var address = new MailAddress(email);

            return address.Address == email;

        }

        catch

        {

            return false;

        }

    }

}


