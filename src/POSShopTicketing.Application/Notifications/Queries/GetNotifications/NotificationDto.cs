using POSShopTicketing.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace POSShopTicketing.Application.Notifications.Queries.GetNotifications
{
    public record NotificationDto
    {
        public Guid Id { get; init; }

        public string Title { get; init; } = string.Empty;

        public string Message { get; init; } = string.Empty;

        public Guid? TicketId { get; init; }

        public bool IsRead { get; init; }

        public DateTime CreatedAt { get; init; }

        public DateTime? ReadAt { get; init; }

        public static NotificationDto FromEntity(Notification entity)
            => new()
            {
                Id = entity.Id,
                Title = entity.Title,
                Message = entity.Message,
                TicketId = entity.TicketId,
                IsRead = entity.IsRead,
                CreatedAt = entity.CreatedAt,
                ReadAt = entity.ReadAt
            };
    }
}
