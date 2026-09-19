using MediatR;
using Microsoft.EntityFrameworkCore;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;

namespace POSShopTicketing.Application.Notifications.Commands.MarkNotificationAsRead;

public record MarkNotificationAsReadCommand(Guid NotificationId)
    : IRequest;

public class MarkNotificationAsReadCommandHandler
    : IRequestHandler<MarkNotificationAsReadCommand>
{
    private readonly IApplicationDbContext _context;

    public MarkNotificationAsReadCommandHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(
        MarkNotificationAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(
                x => x.Id == request.NotificationId,
                cancellationToken);

        if (notification == null)
        {
            throw new NotFoundException(
                nameof(notification),
                request.NotificationId);
        }

        notification.IsRead = true;

        notification.ReadAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}