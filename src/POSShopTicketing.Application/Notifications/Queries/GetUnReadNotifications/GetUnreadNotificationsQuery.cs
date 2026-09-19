using MediatR;
using POSShopTicketing.Application.Notifications.Queries.GetNotifications;

public record GetUnreadNotificationsQuery
    : IRequest<List<NotificationDto>>;