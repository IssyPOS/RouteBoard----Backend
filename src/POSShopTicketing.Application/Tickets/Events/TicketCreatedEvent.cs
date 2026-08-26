using MediatR;

namespace POSShopTicketing.Application.Tickets.Events;

public record TicketCreatedEvent(Guid TenantId, Guid TicketId, string TicketNumber, string Subject, bool IsUnverified) : INotification;
