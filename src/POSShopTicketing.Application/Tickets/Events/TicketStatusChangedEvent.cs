using MediatR;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Tickets.Events;

public record TicketStatusChangedEvent(
    Guid TenantId, Guid TicketId, string TicketNumber, TicketStatus FromStatus, TicketStatus ToStatus) : INotification;
