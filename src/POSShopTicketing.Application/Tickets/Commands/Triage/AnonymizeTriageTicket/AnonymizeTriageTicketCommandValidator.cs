using FluentValidation;

namespace POSShopTicketing.Application.Tickets.Commands.Triage.AnonymizeTriageTicket;

public class AnonymizeTriageTicketCommandValidator : AbstractValidator<AnonymizeTriageTicketCommand>
{
    public AnonymizeTriageTicketCommandValidator()
    {
        RuleFor(v => v.TicketId).NotEmpty();
        RuleFor(v => v.AssignedToTeamMemberId).NotEmpty();
    }
}
