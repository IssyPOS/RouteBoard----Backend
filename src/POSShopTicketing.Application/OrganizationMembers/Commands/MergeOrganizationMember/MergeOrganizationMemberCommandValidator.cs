using FluentValidation;

namespace POSShopTicketing.Application.OrganizationMembers.Commands.MergeOrganizationMember;

public class MergeOrganizationMemberCommandValidator : AbstractValidator<MergeOrganizationMemberCommand>
{
    public MergeOrganizationMemberCommandValidator()
    {
        RuleFor(v => v.OrganizationId).NotEmpty();
        RuleFor(v => v.MemberId).NotEmpty();
        RuleFor(v => v.MergeIntoMemberId).NotEmpty();
    }
}
