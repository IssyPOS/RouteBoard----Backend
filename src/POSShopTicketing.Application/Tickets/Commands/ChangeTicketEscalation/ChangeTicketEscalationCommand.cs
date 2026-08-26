using MediatR;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Tickets.Events;
using POSShopTicketing.Domain.Entities;

namespace POSShopTicketing.Application.Tickets.Commands.ChangeTicketEscalation;

/// <summary>PATCH /tickets/{id}/escalate - Escalated is a boolean flag,
/// not a status (see TicketStatus), set manually for now and SLA-driven
/// automatically once Phase 5's business-hours engine lands.</summary>
public record ChangeTicketEscalationCommand(Guid TicketId, bool Escalated) : IRequest;

public class ChangeTicketEscalationCommandHandler : IRequestHandler<ChangeTicketEscalationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublisher _publisher;

    public ChangeTicketEscalationCommandHandler(IApplicationDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task Handle(ChangeTicketEscalationCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _context.Tickets.FindAsync(new object[] { request.TicketId }, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.TicketId);

        ticket.Escalated = request.Escalated;
        await _context.SaveChangesAsync(cancellationToken);

        if (request.Escalated)
        {
            await _publisher.Publish(new TicketEscalatedEvent(ticket.TenantId, ticket.Id, ticket.TicketNumber), cancellationToken);
        }
    }
}
