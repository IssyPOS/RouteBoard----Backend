using MediatR;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Common.Models;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Tickets.Events;

public class TicketStatusChangedEventHandler : INotificationHandler<TicketStatusChangedEvent>
{
    private readonly IAlertNotifier _alertNotifier;

    public TicketStatusChangedEventHandler(IAlertNotifier alertNotifier)
    {
        _alertNotifier = alertNotifier;
    }

    public Task Handle(TicketStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.ToStatus is not (TicketStatus.Resolved or TicketStatus.Closed or TicketStatus.Open))
        {
            return Task.CompletedTask;
        }

        return _alertNotifier.NotifyAsync(
            new AlertMessage(
                NotificationType.Other,
                $"Ticket {notification.TicketNumber} {notification.ToStatus}",
                $"Ticket {notification.TicketNumber} moved from {notification.FromStatus} to {notification.ToStatus}.",
                notification.TenantId,
                notification.TicketId),
            cancellationToken);
    }
}
