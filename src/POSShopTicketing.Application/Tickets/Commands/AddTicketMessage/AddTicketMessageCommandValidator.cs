using FluentValidation;

namespace POSShopTicketing.Application.Tickets.Commands.AddTicketMessage;

public class AddTicketMessageCommandValidator : AbstractValidator<AddTicketMessageCommand>
{
    public AddTicketMessageCommandValidator()
    {
        RuleFor(v => v.TicketId).NotEmpty();
        RuleFor(v => v.Body).NotEmpty().MaximumLength(8000);
        RuleFor(v => v.Direction).IsInEnum();
    }
}
