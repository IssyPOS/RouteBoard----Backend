using FluentValidation;

namespace POSShopTicketing.Application.OrganizationMembers.Commands.CreateOrganizationMember;

public class CreateOrganizationContactCommandValidator : AbstractValidator<CreateOrganizationContactCommand>
{
    public CreateOrganizationContactCommandValidator()
    {
        RuleFor(v => v.OrganizationId).NotEmpty();
        RuleFor(v => v.FirstName).NotEmpty().MaximumLength(200);
        RuleFor(v => v.LastName).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(v => v.Phone).MaximumLength(30);
    }
}
