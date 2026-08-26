using MediatR;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Common.Models;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Tickets.Events;

public class TicketAssignedEventHandler : INotificationHandler<TicketAssignedEvent>
{
    private readonly IAlertNotifier _alertNotifier;

    public TicketAssignedEventHandler(IAlertNotifier alertNotifier)
    {
        _alertNotifier = alertNotifier;
    }

    public Task Handle(TicketAssignedEvent notification, CancellationToken cancellationToken) =>
        _alertNotifier.NotifyAsync(
            new AlertMessage(
                NotificationType.TicketAssigned,
                $"Ticket {notification.TicketNumber} assigned to you",
                $"Ticket {notification.TicketNumber} was assigned to you.",
                notification.TenantId,
                notification.TicketId,
                notification.AssignedToTeamMemberId),
            cancellationToken);
}
