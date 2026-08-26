using FluentValidation;

namespace POSShopTicketing.Application.Tickets.Commands.ChangeTicketPriority;

public class ChangeTicketPriorityCommandValidator : AbstractValidator<ChangeTicketPriorityCommand>
{
    public ChangeTicketPriorityCommandValidator()
    {
        RuleFor(v => v.TicketId).NotEmpty();
        RuleFor(v => v.Priority).IsInEnum();
    }
}
