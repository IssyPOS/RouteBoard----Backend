using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Notifications.Queries.GetNotifications;

namespace POSShopTicketing.Application.Notifications.Queries.GetUnreadNotifications;

public class GetUnreadNotificationsQueryHandler
    : IRequestHandler<GetUnreadNotificationsQuery, List<NotificationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetUnreadNotificationsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<NotificationDto>> Handle(
        GetUnreadNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var notifications = await _context.Notifications
            .Where(x =>
                x.TeamMemberId == _currentUserService.TeamMemberId &&
                !x.IsRead)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return notifications.Select(x => new NotificationDto
        {
            Id = x.Id,
            TicketId = x.TicketId,
            Title = x.Title,
            Message = x.Message,
            IsRead = x.IsRead,
            CreatedAt = x.CreatedAt,
            ReadAt = x.ReadAt
        }).ToList();
    }
}