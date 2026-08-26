using FluentValidation;

namespace POSShopTicketing.Application.Tickets.Commands.ChangeTicketStatus;

public class ChangeTicketStatusCommandValidator : AbstractValidator<ChangeTicketStatusCommand>
{
    public ChangeTicketStatusCommandValidator()
    {
        RuleFor(v => v.TicketId).NotEmpty();
        RuleFor(v => v.Status).IsInEnum();
        RuleFor(v => v.Note).MaximumLength(1000);
    }
}
