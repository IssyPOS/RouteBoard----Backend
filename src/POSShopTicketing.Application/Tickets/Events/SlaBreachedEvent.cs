using MediatR;

namespace POSShopTicketing.Application.Tickets.Events;

/// <summary>Raised by the Hangfire SLA recurring job when a ticket
/// crosses its DueAt without being Resolved/Closed.</summary>
public record SlaBreachedEvent(Guid TenantId, Guid TicketId, string TicketNumber) : INotification;
