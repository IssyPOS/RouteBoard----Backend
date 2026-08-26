using FluentValidation;

namespace POSShopTicketing.Application.Tickets.Commands.Triage.RejectTriageTicket;

public class RejectTriageTicketCommandValidator : AbstractValidator<RejectTriageTicketCommand>
{
    public RejectTriageTicketCommandValidator()
    {
        RuleFor(v => v.TicketId).NotEmpty();
        RuleFor(v => v.Reason).MaximumLength(500);
    }
}
