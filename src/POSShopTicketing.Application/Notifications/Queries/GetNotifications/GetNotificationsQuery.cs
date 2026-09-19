using MediatR;
using POSShopTicketing.Application.Notifications.Queries.GetNotifications;

public record GetNotificationsQuery : IRequest<List<NotificationDto>>;