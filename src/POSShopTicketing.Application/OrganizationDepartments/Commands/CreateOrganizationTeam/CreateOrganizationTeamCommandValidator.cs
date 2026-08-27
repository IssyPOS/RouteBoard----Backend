using FluentValidation;

namespace POSShopTicketing.Application.OrganizationTeams.Commands.CreateOrganizationTeam;

public class CreateOrganizationTeamCommandValidator : AbstractValidator<CreateOrganizationDepartmentCommand>
{
    public CreateOrganizationTeamCommandValidator()
    {
        RuleFor(v => v.OrganizationId).NotEmpty();
        RuleFor(v => v.Name).NotEmpty().MaximumLength(100);
    }
}
