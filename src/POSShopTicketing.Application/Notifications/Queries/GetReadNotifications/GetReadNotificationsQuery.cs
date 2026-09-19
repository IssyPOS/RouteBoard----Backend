using MediatR;
using POSShopTicketing.Application.Notifications.Queries.GetNotifications;

public record GetReadNotificationsQuery
    : IRequest<List<NotificationDto>>;