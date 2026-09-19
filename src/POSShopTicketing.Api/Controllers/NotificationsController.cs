using Microsoft.AspNetCore.Mvc;
using POSShopTicketing.Application.Notifications.Commands.MarkNotificationAsRead;
using POSShopTicketing.Application.Notifications.Queries.GetNotifications;
using POSShopTicketing.Shared.Wrappers;

namespace POSShopTicketing.Api.Controllers;

public class NotificationsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> Get()
    {
        var result = await Mediator.Send(
            new GetNotificationsQuery());

        return Ok(
            ApiResponse<List<NotificationDto>>
                .Success(result));
    }

    [HttpGet("unread")]
    public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> GetUnread()
    {
        var result = await Mediator.Send(
            new GetUnreadNotificationsQuery());

        return Ok(
            ApiResponse<List<NotificationDto>>
                .Success(result));
    }

    [HttpGet("read")]
    public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> GetRead()
    {
        var result = await Mediator.Send(
            new GetReadNotificationsQuery());

        return Ok(
            ApiResponse<List<NotificationDto>>
                .Success(result));
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<ActionResult<ApiResponse<object>>> MarkAsRead(Guid id)
    {
        await Mediator.Send(
            new MarkNotificationAsReadCommand(id));

        return Ok(
            ApiResponse<object>
                .Success(new { }, "Notification marked as read."));
    }

    [HttpPatch("read-all")]
    public async Task<ActionResult<ApiResponse<object>>> MarkAllAsRead()
    {
        await Mediator.Send(
            new MarkAllNotificationsAsReadCommand());

        return Ok(
            ApiResponse<object>
                .Success(new { }, "All notifications marked as read."));
    }
}