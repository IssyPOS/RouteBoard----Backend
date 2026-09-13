using FluentValidation;

namespace POSShopTicketing.Application.OrganizationTeams.Commands.CreateOrganizationTeam;

public class CreateOrganizationDepartmentCommandValidator : AbstractValidator<CreateOrganizationDepartmentCommand>
{
    public CreateOrganizationDepartmentCommandValidator()
    {
        RuleFor(v => v.OrganizationId).NotEmpty();
        RuleFor(v => v.Name).NotEmpty().MaximumLength(100);
    }
}
