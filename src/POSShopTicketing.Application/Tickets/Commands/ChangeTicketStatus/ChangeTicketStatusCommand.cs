using MediatR;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Tickets.Events;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;
using POSShopTicketing.Domain.Exceptions;

namespace POSShopTicketing.Application.Tickets.Commands.ChangeTicketStatus;

/// <summary>
/// PATCH /tickets/{id}/status - moves a ticket along
/// Unverified -> New -> Open -> Pending <-> Open -> Resolved -> Closed.
/// Every transition is written to TicketStatusHistory (status tracking);
/// ResolvedAt/ClosedAt are stamped or cleared automatically depending on
/// direction.
/// </summary>
public record ChangeTicketStatusCommand(Guid TicketId, TicketStatus Status, string? Note = null) : IRequest;

public class ChangeTicketStatusCommandHandler : IRequestHandler<ChangeTicketStatusCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPublisher _publisher;

    public ChangeTicketStatusCommandHandler(
        IApplicationDbContext context,
        IDateTime dateTime,
        ICurrentUserService currentUserService,
        IPublisher publisher)
    {
        _context = context;
        _dateTime = dateTime;
        _currentUserService = currentUserService;
        _publisher = publisher;
    }

    public async Task Handle(ChangeTicketStatusCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _context.Tickets.FindAsync(new object[] { request.TicketId }, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.TicketId);

        if (ticket.Status == request.Status)
        {
            throw new DomainException($"Ticket {ticket.TicketNumber} is already {request.Status}.");
        }

        var previousStatus = ticket.Status;
        var now = _dateTime.Now;

        ticket.Status = request.Status;
        ticket.ResolvedAt = request.Status == TicketStatus.Resolved ? now : ticket.ResolvedAt;
        ticket.ClosedAt = request.Status == TicketStatus.Closed ? now : null;

        if (request.Status is TicketStatus.New or TicketStatus.Open or TicketStatus.Pending)
        {
            ticket.ResolvedAt = null;
        }

        _context.TicketStatusHistories.Add(new TicketStatusHistory
        {
            TenantId = ticket.TenantId,
            TicketId = ticket.Id,
            FromStatus = previousStatus,
            ToStatus = request.Status,
            ChangedByTeamMemberId = _currentUserService.TeamMemberId,
            ChangedAt = now,
            Note = request.Note
        });

        await _context.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new TicketStatusChangedEvent(ticket.TenantId, ticket.Id, ticket.TicketNumber, previousStatus, request.Status),
            cancellationToken);
    }
}
