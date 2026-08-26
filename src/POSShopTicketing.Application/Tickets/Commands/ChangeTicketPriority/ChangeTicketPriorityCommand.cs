using MediatR;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Domain.Entities;

namespace POSShopTicketing.Application.Tickets.Commands.ChangeTicketPriority;

/// <summary>PATCH /tickets/{id}/priority - also recomputes DueAt against
/// the new Priority's SlaPolicy, measured from the ticket's original
/// CreatedAt (not "now"), so raising priority tightens the deadline
/// rather than resetting the clock.</summary>
public record ChangeTicketPriorityCommand(Guid TicketId, Domain.Enums.TicketPriority Priority) : IRequest;

public class ChangeTicketPriorityCommandHandler : IRequestHandler<ChangeTicketPriorityCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISlaCalculator _slaCalculator;

    public ChangeTicketPriorityCommandHandler(IApplicationDbContext context, ISlaCalculator slaCalculator)
    {
        _context = context;
        _slaCalculator = slaCalculator;
    }

    public async Task Handle(ChangeTicketPriorityCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _context.Tickets.FindAsync(new object[] { request.TicketId }, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.TicketId);

        ticket.Priority = request.Priority;
        ticket.DueAt = await _slaCalculator.CalculateDueDateAsync(ticket.TenantId, request.Priority, ticket.CreatedAt, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
