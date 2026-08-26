namespace POSShopTicketing.Application.Tickets.Commands.IngestInboundEmail;

public record IngestInboundEmailResult(Guid TicketId, string TicketNumber, Guid MessageId, bool IsNewTicket, bool IsUnverified);
