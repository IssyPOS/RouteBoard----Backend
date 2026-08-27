using MediatR;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Tickets.Events;
using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;
using POSShopTicketing.Domain.Exceptions;

namespace POSShopTicketing.Application.Tickets.Commands.Triage.RegisterTriageTicket;

/// <summary>
/// Triage action 1/4: "Register" - create a new Organization Member on
/// an existing org, an existing org team, or a brand-new org on the
/// spot. The ticket links retroactively, moves to New, and future
/// emails from that address auto-route through normal presets because a
/// real Organization Member now exists to match against.
/// </summary>
public record RegisterTriageTicketCommand : IRequest
{
    public Guid TicketId { get; init; }
    public Guid? OrganizationId { get; init; }
    public string? NewOrganizationName { get; init; }
    public Guid? OrganizationTeamId { get; init; }
    public string? NewOrganizationTeamName { get; init; }
    public string MemberFullName { get; init; } = string.Empty;
}

public class RegisterTriageTicketCommandHandler : IRequestHandler<RegisterTriageTicketCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITicketAssignmentService _assignmentService;
    private readonly IDateTime _dateTime;
    private readonly IPublisher _publisher;

    public RegisterTriageTicketCommandHandler(
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

    public async Task Handle(RegisterTriageTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _context.Tickets.FindAsync(new object[] { request.TicketId }, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.TicketId);

        if (ticket.Status != TicketStatus.Unverified)
        {
            throw new DomainException($"Ticket {ticket.TicketNumber} is not in the triage queue.");
        }

        Organization organization;
        if (request.OrganizationId.HasValue)
        {
            organization = await _context.Organizations.FindAsync(new object[] { request.OrganizationId.Value }, cancellationToken)
                ?? throw new NotFoundException(nameof(Organization), request.OrganizationId.Value);
        }
        else
        {
            organization = new Organization { TenantId = ticket.TenantId, Name = request.NewOrganizationName!.Trim() };
            _context.Organizations.Add(organization);
        }

        OrganizationDepartment? department = null;
        if (request.OrganizationTeamId.HasValue)
        {
            department = await _context.OrganizationDepartments.FindAsync(new object[] { request.OrganizationTeamId.Value }, cancellationToken)
                ?? throw new NotFoundException(nameof(OrganizationDepartment), request.OrganizationTeamId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(request.NewOrganizationTeamName))
        {
            department = new OrganizationDepartment { TenantId = ticket.TenantId, Organization = organization, Name = request.NewOrganizationTeamName.Trim() };
            _context.OrganizationDepartments.Add(department);
        }

        var contact = new OrganizationContact
        {
            TenantId = ticket.TenantId,
            Organization = organization,
            OrganizationDepartment = department,
            FullName = request.MemberFullName.Trim(),
            Email = ticket.RawSenderEmail
        };

        _context.OrganizationContacts.Add(contact);

        var now = _dateTime.Now;
        var assignedToTeamMemberId = await _assignmentService.ResolveAssigneeAsync(
            ticket.TenantId, organization.Id, department?.Id, contact.Id, cancellationToken);

        ApplyTriageDecision(
            ticket, TicketStatus.New,
            organization.Id, department?.Id, contact.Id, assignedToTeamMemberId,
            $"Registered as new Organization Contact \"{contact.FullName}\" on \"{organization.Name}\"",
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

    internal static void ApplyTriageDecision(
        Ticket ticket, TicketStatus newStatus,
        Guid? organizationId, Guid? organizationTeamId, Guid? organizationMemberId, Guid? assignedToTeamMemberId,
        string note, Guid? changedByTeamMemberId, DateTime now)
    {
        ticket.OrganizationId = organizationId;
        ticket.OrganizationDepartmentId = organizationTeamId;
        ticket.OrganizationContactId = organizationMemberId;
        ticket.AssignedToTeamMemberId = assignedToTeamMemberId;
        ticket.Status = newStatus;
        ticket.ClosedAt = newStatus == TicketStatus.Closed ? now : null;

        ticket.StatusHistory.Add(new TicketStatusHistory
        {
            TenantId = ticket.TenantId,
            TicketId = ticket.Id,
            FromStatus = TicketStatus.Unverified,
            ToStatus = newStatus,
            ChangedByTeamMemberId = changedByTeamMemberId,
            ChangedAt = now,
            Note = note
        });
    }
}
