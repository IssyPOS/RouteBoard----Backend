using FluentValidation;

namespace POSShopTicketing.Application.Notifications.Commands.MarkNotificationAsRead;

public class MarkNotificationAsReadCommandValidator
    : AbstractValidator<MarkNotificationAsReadCommand>
{
    public MarkNotificationAsReadCommandValidator()
    {
        RuleFor(x => x.NotificationId)
            .NotEmpty();
    }
}