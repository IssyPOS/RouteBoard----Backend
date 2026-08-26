using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Common.Models;

public record AlertMessage(
    NotificationType Type,
    string Title,
    string Body,
    Guid? TenantId = null,
    Guid? TicketId = null,
    Guid? TeamMemberId = null);
