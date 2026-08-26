using MediatR;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Common.Models;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Tickets.Events;

public class TicketCreatedEventHandler : INotificationHandler<TicketCreatedEvent>
{
    private readonly IAlertNotifier _alertNotifier;

    public TicketCreatedEventHandler(IAlertNotifier alertNotifier)
    {
        _alertNotifier = alertNotifier;
    }

    public Task Handle(TicketCreatedEvent notification, CancellationToken cancellationToken) =>
        _alertNotifier.NotifyAsync(
            new AlertMessage(
                NotificationType.Other,
                notification.IsUnverified ? $"New Unverified ticket {notification.TicketNumber}" : $"New ticket {notification.TicketNumber}",
                notification.Subject,
                notification.TenantId,
                notification.TicketId),
            cancellationToken);
}
