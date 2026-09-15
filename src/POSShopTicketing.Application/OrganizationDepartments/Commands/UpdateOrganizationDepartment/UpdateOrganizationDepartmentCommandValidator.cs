using FluentValidation;

namespace POSShopTicketing.Application.Organizations.Commands.UpdateOrganizationDepartment;

public class UpdateOrganizationDepartmentCommandValidator : AbstractValidator<UpdateOrganizationDepartmentCommand>
{
    public UpdateOrganizationDepartmentCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(100);
    }
}

