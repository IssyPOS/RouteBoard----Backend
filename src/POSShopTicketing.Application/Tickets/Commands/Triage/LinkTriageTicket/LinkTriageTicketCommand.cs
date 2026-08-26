using MediatR;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Tickets.Commands.Triage.RegisterTriageTicket;
using POSShopTicketing.Application.Tickets.Events;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;
using POSShopTicketing.Domain.Exceptions;

namespace POSShopTicketing.Application.Tickets.Commands.Triage.LinkTriageTicket;

/// <summary>Triage action 2/4: "Link" - attach the sender to an
/// *existing* Organization Member (an alternate or typo'd address of
/// someone already known). Same effect as Register, without creating a
/// duplicate member.</summary>
public record LinkTriageTicketCommand(Guid TicketId, Guid OrganizationMemberId) : IRequest;

public class LinkTriageTicketCommandHandler : IRequestHandler<LinkTriageTicketCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITicketAssignmentService _assignmentService;
    private readonly IDateTime _dateTime;
    private readonly IPublisher _publisher;

    public LinkTriageTicketCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ITicketAssignmentService assignmentService,
        IDateTime dateTime,
        IPublisher publisher)
    {
        _context = context;
        _currentUserService = currentUserService;
        _assignmentService = assignmentService;
        _dateTime = dateTime;
        _publisher = publisher;
    }

    public async Task Handle(LinkTriageTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _context.Tickets.FindAsync(new object[] { request.TicketId }, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.TicketId);

        if (ticket.Status != TicketStatus.Unverified)
        {
            throw new DomainException($"Ticket {ticket.TicketNumber} is not in the triage queue.");
        }

        var member = await _context.OrganizationMembers.FindAsync(new object[] { request.OrganizationMemberId }, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationMember), request.OrganizationMemberId);

        var now = _dateTime.Now;
        var assignedToTeamMemberId = await _assignmentService.ResolveAssigneeAsync(
            ticket.TenantId, member.OrganizationId, member.OrganizationTeamId, member.Id, cancellationToken);

        RegisterTriageTicketCommandHandler.ApplyTriageDecision(
            ticket, TicketStatus.New,
            member.OrganizationId, member.OrganizationTeamId, member.Id, assignedToTeamMemberId,
            $"Linked to existing Organization Member \"{member.FullName}\"",
            _currentUserService.TeamMemberId, now);

        await _context.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new TicketStatusChangedEvent(ticket.TenantId, ticket.Id, ticket.TicketNumber, TicketStatus.Unverified, TicketStatus.New),
            cancellationToken);

        if (assignedToTeamMemberId.HasValue)
        {
            await _publisher.Publish(
                new TicketAssignedEvent(ticket.TenantId, ticket.Id, ticket.TicketNumber, assignedToTeamMemberId.Value),
                cancellationToken);
        }
    }
}
