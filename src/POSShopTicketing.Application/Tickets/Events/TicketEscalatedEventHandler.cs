using MediatR;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Common.Models;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Tickets.Events;

public class TicketEscalatedEventHandler : INotificationHandler<TicketEscalatedEvent>
{
    private readonly IAlertNotifier _alertNotifier;

    public TicketEscalatedEventHandler(IAlertNotifier alertNotifier)
    {
        _alertNotifier = alertNotifier;
    }

    public Task Handle(TicketEscalatedEvent notification, CancellationToken cancellationToken) =>
        _alertNotifier.NotifyAsync(
            new AlertMessage(
                NotificationType.TicketEscalated,
                $"Ticket {notification.TicketNumber} escalated",
                $"Ticket {notification.TicketNumber} was marked escalated.",
                notification.TenantId,
                notification.TicketId),
            cancellationToken);
}
