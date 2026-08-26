using MediatR;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Tickets.Commands.Triage.RegisterTriageTicket;
using POSShopTicketing.Application.Tickets.Events;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;
using POSShopTicketing.Domain.Exceptions;

namespace POSShopTicketing.Application.Tickets.Commands.Triage.RejectTriageTicket;

/// <summary>Triage action 4/4: "Reject" - close without engaging (spam
/// or out-of-scope mail).</summary>
public record RejectTriageTicketCommand(Guid TicketId, string? Reason) : IRequest;

public class RejectTriageTicketCommandHandler : IRequestHandler<RejectTriageTicketCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;
    private readonly IPublisher _publisher;

    public RejectTriageTicketCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDateTime dateTime,
        IPublisher publisher)
    {
        _context = context;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _publisher = publisher;
    }

    public async Task Handle(RejectTriageTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _context.Tickets.FindAsync(new object[] { request.TicketId }, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.TicketId);

        if (ticket.Status != TicketStatus.Unverified)
        {
            throw new DomainException($"Ticket {ticket.TicketNumber} is not in the triage queue.");
        }

        var now = _dateTime.Now;

        RegisterTriageTicketCommandHandler.ApplyTriageDecision(
            ticket, TicketStatus.Closed,
            organizationId: null, organizationTeamId: null, organizationMemberId: null,
            assignedToTeamMemberId: null,
            string.IsNullOrWhiteSpace(request.Reason) ? "Rejected (spam or out-of-scope)" : $"Rejected: {request.Reason}",
            _currentUserService.TeamMemberId, now);

        await _context.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new TicketStatusChangedEvent(ticket.TenantId, ticket.Id, ticket.TicketNumber, TicketStatus.Unverified, TicketStatus.Closed),
            cancellationToken);
    }
}
