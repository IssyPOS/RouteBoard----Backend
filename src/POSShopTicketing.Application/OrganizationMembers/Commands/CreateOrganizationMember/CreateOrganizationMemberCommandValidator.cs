using FluentValidation;

namespace POSShopTicketing.Application.OrganizationMembers.Commands.CreateOrganizationMember;

public class CreateOrganizationMemberCommandValidator : AbstractValidator<CreateOrganizationMemberCommand>
{
    public CreateOrganizationMemberCommandValidator()
    {
        RuleFor(v => v.OrganizationId).NotEmpty();
        RuleFor(v => v.FullName).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(v => v.Phone).MaximumLength(30);
    }
}
