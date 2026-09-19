using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Notifications.Queries.GetNotifications;

namespace POSShopTicketing.Application.Notifications.Queries.GetReadNotifications;

public class GetReadNotificationsQueryHandler
    : IRequestHandler<GetReadNotificationsQuery, List<NotificationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetReadNotificationsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<NotificationDto>> Handle(
        GetReadNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Notifications
            .Where(x =>
                x.TeamMemberId == _currentUserService.TeamMemberId &&
                x.IsRead)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new NotificationDto
            {
                Id = x.Id,
                TicketId = x.TicketId,
                Title = x.Title,
                Message = x.Message,
                IsRead = x.IsRead,
                CreatedAt = x.CreatedAt,
                ReadAt = x.ReadAt
            })
            .ToListAsync(cancellationToken);
    }
}