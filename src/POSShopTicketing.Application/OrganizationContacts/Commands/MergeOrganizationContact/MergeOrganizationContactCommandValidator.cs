using FluentValidation;

namespace POSShopTicketing.Application.OrganizationMembers.Commands.MergeOrganizationMember;

public class MergeOrganizationContactCommandValidator : AbstractValidator<MergeOrganizationContactCommand>
{
    public MergeOrganizationContactCommandValidator()
    {
        RuleFor(v => v.OrganizationId).NotEmpty();
        RuleFor(v => v.ContactId).NotEmpty();
        RuleFor(v => v.MergeIntoContactId).NotEmpty();
    }
}
