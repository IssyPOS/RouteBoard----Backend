using POSShopTicketing.Domain.Entities;
using POSShopTicketing.Domain.Enums;

namespace POSShopTicketing.Application.Platform.Queries.GetTenants;

public record TenantDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string TicketPrefix { get; init; } = string.Empty;
    public TenantPlan Plan { get; init; }
    public TenantStatus Status { get; init; }
    public int TeamMemberCount { get; init; }
    public DateTime CreatedAt { get; init; }

    public static TenantDto FromEntity(Tenant entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Slug = entity.Slug,
        TicketPrefix = entity.TicketPrefix,
        Plan = entity.Plan,
        Status = entity.Status,
        TeamMemberCount = entity.TeamMembers.Count,
        CreatedAt = entity.CreatedAt
    };
}
