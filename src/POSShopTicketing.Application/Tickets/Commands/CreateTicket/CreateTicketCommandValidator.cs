using FluentValidation;

namespace POSShopTicketing.Application.Tickets.Commands.CreateTicket;

public class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketCommandValidator()
    {
        RuleFor(v => v.Subject).NotEmpty().MaximumLength(250);
        RuleFor(v => v.InitialMessageBody).NotEmpty().MaximumLength(8000);
        RuleFor(v => v.Priority!.Value).IsInEnum().When(v => v.Priority.HasValue);
    }
}
