using FluentValidation;

namespace POSShopTicketing.Application.Mailboxes.Commands.CreateMailbox;

public class CreateMailboxCommandValidator : AbstractValidator<CreateMailboxCommand>
{
    public CreateMailboxCommandValidator()
    {
        // Omitted entirely -> zero-setup system-provided address (see
        // the handler). Only validated as an email when actually supplied.
        RuleFor(v => v.EmailAddress)
            .EmailAddress().MaximumLength(200)
            .When(v => !string.IsNullOrWhiteSpace(v.EmailAddress));

        RuleFor(v => v.Provider).IsInEnum();
    }
}
