using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.TeamMembers.Queries.GetTeamMembers;

public record TeamMemberDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public TeamMemberRole Role { get; init; }
    public TeamMemberStatus Status { get; init; }
    public int OpenTicketCount { get; init; }
    public DateTime CreatedAt { get; init; }

    public static TeamMemberDto FromEntity(TeamMember entity) => new()
    {
        Id = entity.Id,
        Email = entity.Email,
        FullName = entity.FullName,
        Role = entity.Role,
        Status = entity.Status,
        OpenTicketCount = entity.AssignedTickets.Count(t => t.Status != TicketStatus.Closed && t.Status != TicketStatus.Resolved),
        CreatedAt = entity.CreatedAt
    };
}
