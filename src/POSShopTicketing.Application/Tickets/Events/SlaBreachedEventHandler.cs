using MediatR;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Common.Models;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Tickets.Events;

public class SlaBreachedEventHandler : INotificationHandler<SlaBreachedEvent>
{
    private readonly IAlertNotifier _alertNotifier;

    public SlaBreachedEventHandler(IAlertNotifier alertNotifier)
    {
        _alertNotifier = alertNotifier;
    }

    public Task Handle(SlaBreachedEvent notification, CancellationToken cancellationToken) =>
        _alertNotifier.NotifyAsync(
            new AlertMessage(
                NotificationType.SlaBreached,
                $"SLA breached: {notification.TicketNumber}",
                $"Ticket {notification.TicketNumber} missed its resolution SLA target.",
                notification.TenantId,
                notification.TicketId),
            cancellationToken);
}
