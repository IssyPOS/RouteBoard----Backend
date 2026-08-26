using FluentValidation;

namespace POSShopTicketing.Application.Tickets.Commands.Triage.LinkTriageTicket;

public class LinkTriageTicketCommandValidator : AbstractValidator<LinkTriageTicketCommand>
{
    public LinkTriageTicketCommandValidator()
    {
        RuleFor(v => v.TicketId).NotEmpty();
        RuleFor(v => v.OrganizationMemberId).NotEmpty();
    }
}
