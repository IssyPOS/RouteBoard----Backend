using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Tickets.Queries.GetTickets;

public class TicketDto
{
    public Guid Id { get; init; }
    public string TicketNumber { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public TicketStatus Status { get; init; }
    public TicketPriority Priority { get; init; }
    public TicketSource Source { get; init; }
    public bool Escalated { get; init; }

    public Guid? OrganizationId { get; init; }
    public string? OrganizationName { get; init; }
    public Guid? OrganizationTeamId { get; init; }
    public string? OrganizationTeamName { get; init; }
    public Guid? OrganizationMemberId { get; init; }
    public string? OrganizationMemberName { get; init; }
    public Guid CreatorId { get; init; }
    public string? CreatorName { get; init; }
    public string RawSenderEmail { get; init; } = string.Empty;

    public Guid? AssignedToTeamMemberId { get; init; }
    public string? AssignedToTeamMemberName { get; init; }

    public DateTime? FirstResponseAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
    public DateTime? ClosedAt { get; init; }
    public DateTime? DueAt { get; init; }
    public bool IsSlaBreached { get; init; }

    public DateTime CreatedAt { get; init; }

    public static TicketDto FromEntity(Ticket entity, DateTime now) => new()
    {
        Id = entity.Id,
        TicketNumber = entity.TicketNumber,
        Subject = entity.Subject,
        Status = entity.Status,
        Priority = entity.Priority,
        Source = entity.Source,
        Escalated = entity.Escalated,
        OrganizationId = entity.OrganizationId,
        OrganizationName = entity.Organization?.Name,
        OrganizationTeamId = entity.OrganizationDepartmentId,
        OrganizationTeamName = entity.OrganizationDepartment?.Name,
        OrganizationMemberId = entity.OrganizationContactId,
        OrganizationMemberName = entity.OrganizationContact?.FullName,
        CreatorId = entity.CreatorId,
        CreatorName = entity.CreatorName,
        RawSenderEmail = entity.RawSenderEmail,
        AssignedToTeamMemberId = entity.AssignedToTeamMemberId,
        AssignedToTeamMemberName = entity.AssignedToTeamMember?.FullName,
        FirstResponseAt = entity.FirstResponseAt,
        ResolvedAt = entity.ResolvedAt,
        ClosedAt = entity.ClosedAt,
        DueAt = entity.DueAt,
        IsSlaBreached = entity.Status is not (TicketStatus.Resolved or TicketStatus.Closed)
            && entity.DueAt is not null && now > entity.DueAt,
        CreatedAt = entity.CreatedAt
    };
}
