using MediatR;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Tickets.Commands.Triage.RegisterTriageTicket;
using POSShopTicketing.Application.Tickets.Events;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;
using POSShopTicketing.Domain.Exceptions;

namespace POSShopTicketing.Application.Tickets.Commands.Triage.AnonymizeTriageTicket;

/// <summary>Triage action 3/4: "Assign as anonymous" - leave org/member
/// blank, hand the ticket directly to an Agent, and let it proceed as an
/// unattributed one-off, for a sender not worth registering.</summary>
public record AnonymizeTriageTicketCommand(Guid TicketId, Guid AssignedToTeamMemberId) : IRequest;

public class AnonymizeTriageTicketCommandHandler : IRequestHandler<AnonymizeTriageTicketCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;
    private readonly IPublisher _publisher;

    public AnonymizeTriageTicketCommandHandler(
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

    public async Task Handle(AnonymizeTriageTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _context.Tickets.FindAsync(new object[] { request.TicketId }, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.TicketId);

        if (ticket.Status != TicketStatus.Unverified)
        {
            throw new DomainException($"Ticket {ticket.TicketNumber} is not in the triage queue.");
        }

        var agent = await _context.TeamMembers.FindAsync(new object[] { request.AssignedToTeamMemberId }, cancellationToken)
            ?? throw new NotFoundException(nameof(TeamMember), request.AssignedToTeamMemberId);

        if (agent.Status != TeamMemberStatus.Active)
        {
            throw new NotFoundException(nameof(TeamMember), request.AssignedToTeamMemberId);
        }

        var now = _dateTime.Now;

        RegisterTriageTicketCommandHandler.ApplyTriageDecision(
            ticket, TicketStatus.New,
            organizationId: null, organizationTeamId: null, organizationMemberId: null,
            assignedToTeamMemberId: agent.Id,
            $"Assigned as anonymous one-off to {agent.FullName}",
            _currentUserService.TeamMemberId, now);

        await _context.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new TicketStatusChangedEvent(ticket.TenantId, ticket.Id, ticket.TicketNumber, TicketStatus.Unverified, TicketStatus.New),
            cancellationToken);

        await _publisher.Publish(
            new TicketAssignedEvent(ticket.TenantId, ticket.Id, ticket.TicketNumber, agent.Id),
            cancellationToken);
    }
}
