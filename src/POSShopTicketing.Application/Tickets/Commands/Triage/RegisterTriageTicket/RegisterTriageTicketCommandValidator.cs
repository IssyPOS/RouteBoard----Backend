using FluentValidation;

namespace POSShopTicketing.Application.Tickets.Commands.Triage.RegisterTriageTicket;

public class RegisterTriageTicketCommandValidator : AbstractValidator<RegisterTriageTicketCommand>
{
    public RegisterTriageTicketCommandValidator()
    {
        RuleFor(v => v.TicketId).NotEmpty();
        RuleFor(v => v.MemberFullName).NotEmpty().MaximumLength(200);

        RuleFor(v => v)
            .Must(v => v.OrganizationId.HasValue || !string.IsNullOrWhiteSpace(v.NewOrganizationName))
            .WithMessage("Either an existing OrganizationId or a NewOrganizationName is required.");
    }
}
