using MediatR;

namespace POSShopTicketing.Application.Tickets.Events;

public record TicketEscalatedEvent(Guid TenantId, Guid TicketId, string TicketNumber) : INotification;
