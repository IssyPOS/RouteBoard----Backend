using FluentValidation;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Organizations.Commands.CreateOrganization;

public class CreateOrganizationCommandValidator : AbstractValidator<CreateOrganizationCommand>
{
    public CreateOrganizationCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Status).IsInEnum().NotEqual(OrganizationStatus.Active);
        
    }
}
