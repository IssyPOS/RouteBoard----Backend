using FluentValidation;

namespace POSShopTicketing.Application.Tickets.Commands.ChangeTicketAssignee;

public class ChangeTicketAssigneeCommandValidator : AbstractValidator<ChangeTicketAssigneeCommand>
{
    public ChangeTicketAssigneeCommandValidator()
    {
        RuleFor(v => v.TicketId).NotEmpty();
    }
}
