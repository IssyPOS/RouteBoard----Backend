using FluentValidation;

namespace POSShopTicketing.Application.OrganizationMembers.Commands.MergeOrganizationMember;

public class MergeOrganizationMemberCommandValidator : AbstractValidator<MergeOrganizationContactCommand>
{
    public MergeOrganizationMemberCommandValidator()
    {
        RuleFor(v => v.OrganizationId).NotEmpty();
        RuleFor(v => v.ContactId).NotEmpty();
        RuleFor(v => v.MergeIntoContactId).NotEmpty();
    }
}
